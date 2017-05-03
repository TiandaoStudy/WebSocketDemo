import * as Messages from './Messages';
export type ConnectionClosedDelegate = (error?: Error) => void;
export type OnDataReceivedDelegate = (data: any) => void;
export type CallableMethod = (...args: any[]) => void;

export class HubConnection {
    private webSocket: WebSocket;
    private subscriptions: Map<string, CallableMethod>;

    public constructor(public url: string) {
        this.subscriptions = new Map<string, CallableMethod>();
    }

    public connect(): Promise<void> {
        return new Promise<void>((resolve, reject) => {
            let webSocket = new WebSocket(this.url);

            webSocket.onerror = (error: ErrorEvent) => {
                reject(error);
            };

            webSocket.onopen = (event: Event) => {
                this.webSocket = webSocket;
                resolve();
            };

            webSocket.onmessage = (message: MessageEvent) => {
                this.handleMessage(message.data);
            };

            webSocket.onclose = (closeEvent: CloseEvent) => {
                if (this.onClosed && this.webSocket) {
                    if (closeEvent.wasClean === false || closeEvent.code !== 1000) {
                        this.onClosed(new Error(
                            `The socket connection closed with status: ${closeEvent.code} (reason: ${closeEvent
                            .reason}).`));
                    } else {
                        this.onClosed();
                    }
                }
            };
        });
    }

    public disconnect(): void {
        if (this.webSocket && this.webSocket.readyState === WebSocket.OPEN) {
            this.webSocket.close();
        }
    }

    public onClosed: ConnectionClosedDelegate;
    public subscribe(channelName: string, method: CallableMethod): void {
        if (!this.subscriptions.has(channelName)) {
            this.subscriptions.set(channelName, method);
            let subscriptionMessage = new Messages.SubscriptionData(channelName);
            this.sendMessage(new Messages.Message(Messages.MessageType.Subscribe, subscriptionMessage));
        } else {
            console.log('This channel is already subscribed to WutFace');
        }
    }

    public unsubscribe(channelName: string): void {
        if (this.subscriptions.has(channelName)) {
            this.subscriptions.delete(channelName);
            let unsubscriptionMessage = new Messages.UnsubscriptionData(channelName);
            this.sendMessage(new Messages.Message(Messages.MessageType.Unsubscribe, unsubscriptionMessage));
        }
    }

    private sendMessage(message: Messages.Message) {
        if (this.webSocket && this.webSocket.readyState === WebSocket.OPEN) {
            this.webSocket.send(JSON.stringify(message));
        }
    }

    private handleMessage(data: any): void {
        let parsedJson: Messages.Message = JSON.parse(data);
        if (parsedJson.messageType === Messages.MessageType.Message) {
            let messageBody = (<Messages.MessageData>parsedJson.data);
            if (this.subscriptions.has(messageBody.topic)) {
                this.subscriptions.get(messageBody.topic).call(this, messageBody.message);
            }
        }
    }
}