export class SubscriptionData {
    constructor(public topic: string) {
    }
};

export class UnsubscriptionData {
    constructor(public topic: string) {
    }
};

export class MessageData {
    constructor(public topic: string, public message: any) {
    }
};

export const enum MessageType {
    Subscribe,
    Unsubscribe,
    Message
};

export class Message {
    constructor(public messageType: MessageType, public data: MessageData | SubscriptionData | UnsubscriptionData) {
    }
};