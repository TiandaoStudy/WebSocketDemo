import * as Messages from './Messages';
export class HubConnection {
    constructor(url) {
        this.url = url;
        this.subscriptions = new Map();
    }
    connect() {
        return new Promise((resolve, reject) => {
            let webSocket = new WebSocket(this.url + "?auth=" + this.authToken);
            webSocket.onerror = (error) => {
                reject(error);
            };
            webSocket.onopen = (event) => {
                this.webSocket = webSocket;
                resolve();
            };
            webSocket.onmessage = (message) => {
                this.handleMessage(message.data);
            };
            webSocket.onclose = (closeEvent) => {
                if (this.onClosed && this.webSocket) {
                    if (closeEvent.wasClean === false || closeEvent.code !== 1000) {
                        this.onClosed(new Error(`The socket connection closed with status: ${closeEvent.code} (reason: ${closeEvent
                            .reason}).`));
                    }
                    else {
                        this.onClosed();
                    }
                }
            };
        });
    }
    setAuthToken(accessToken) {
        this.authToken = accessToken;
    }
    disconnect() {
        if (this.webSocket && this.webSocket.readyState === WebSocket.OPEN) {
            this.webSocket.close();
        }
    }
    subscribe(channelName, method) {
        if (!this.subscriptions.has(channelName)) {
            this.subscriptions.set(channelName, method);
            let subscriptionMessage = new Messages.SubscriptionData(channelName);
            this.sendMessage(new Messages.Message(0 /* Subscribe */, subscriptionMessage));
        }
        else {
            console.log('This channel is already subscribed to WutFace');
        }
    }
    unsubscribe(channelName) {
        if (this.subscriptions.has(channelName)) {
            this.subscriptions.delete(channelName);
            let unsubscriptionMessage = new Messages.UnsubscriptionData(channelName);
            this.sendMessage(new Messages.Message(1 /* Unsubscribe */, unsubscriptionMessage));
        }
    }
    sendMessage(message) {
        if (this.webSocket && this.webSocket.readyState === WebSocket.OPEN) {
            this.webSocket.send(JSON.stringify(message));
        }
    }
    handleMessage(data) {
        let parsedJson = JSON.parse(data);
        if (parsedJson.messageType === 2 /* Message */) {
            let messageBody = parsedJson.data;
            if (this.subscriptions.has(messageBody.topic)) {
                this.subscriptions.get(messageBody.topic).call(this, messageBody.message);
            }
        }
    }
}
//# sourceMappingURL=HubConnection.js.map