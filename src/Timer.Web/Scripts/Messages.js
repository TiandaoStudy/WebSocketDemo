export class SubscriptionData {
    constructor(topic) {
        this.topic = topic;
    }
}
;
export class UnsubscriptionData {
    constructor(topic) {
        this.topic = topic;
    }
}
;
export class MessageData {
    constructor(topic, message) {
        this.topic = topic;
        this.message = message;
    }
}
;
;
export class Message {
    constructor(messageType, data) {
        this.messageType = messageType;
        this.data = data;
    }
}
;
//# sourceMappingURL=Messages.js.map