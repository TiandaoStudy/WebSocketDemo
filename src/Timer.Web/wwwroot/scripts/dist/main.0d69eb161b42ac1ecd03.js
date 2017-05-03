webpackJsonp([0],{

/***/ 117:
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_0__Messages__ = __webpack_require__(119);

class HubConnection {
    constructor(url) {
        this.url = url;
        this.subscriptions = new Map();
    }
    connect() {
        return new Promise((resolve, reject) => {
            let webSocket = new WebSocket(this.url);
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
    disconnect() {
        if (this.webSocket && this.webSocket.readyState === WebSocket.OPEN) {
            this.webSocket.close();
        }
    }
    subscribe(channelName, method) {
        if (!this.subscriptions.has(channelName)) {
            this.subscriptions.set(channelName, method);
            let subscriptionMessage = new __WEBPACK_IMPORTED_MODULE_0__Messages__["a" /* SubscriptionData */](channelName);
            this.sendMessage(new __WEBPACK_IMPORTED_MODULE_0__Messages__["b" /* Message */](0 /* Subscribe */, subscriptionMessage));
        }
        else {
            console.log('This channel is already subscribed to WutFace');
        }
    }
    unsubscribe(channelName) {
        if (this.subscriptions.has(channelName)) {
            this.subscriptions.delete(channelName);
            let unsubscriptionMessage = new __WEBPACK_IMPORTED_MODULE_0__Messages__["c" /* UnsubscriptionData */](channelName);
            this.sendMessage(new __WEBPACK_IMPORTED_MODULE_0__Messages__["b" /* Message */](1 /* Unsubscribe */, unsubscriptionMessage));
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
/* harmony export (immutable) */ __webpack_exports__["a"] = HubConnection;



/***/ }),

/***/ 119:
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
class SubscriptionData {
    constructor(topic) {
        this.topic = topic;
    }
}
/* harmony export (immutable) */ __webpack_exports__["a"] = SubscriptionData;

;
class UnsubscriptionData {
    constructor(topic) {
        this.topic = topic;
    }
}
/* harmony export (immutable) */ __webpack_exports__["c"] = UnsubscriptionData;

;
class MessageData {
    constructor(topic, message) {
        this.topic = topic;
        this.message = message;
    }
}
/* unused harmony export MessageData */

;
;
class Message {
    constructor(messageType, data) {
        this.messageType = messageType;
        this.data = data;
    }
}
/* harmony export (immutable) */ __webpack_exports__["b"] = Message;

;


/***/ }),

/***/ 120:
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
Object.defineProperty(__webpack_exports__, "__esModule", { value: true });
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_0_jquery__ = __webpack_require__(2);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_0_jquery___default = __webpack_require__.n(__WEBPACK_IMPORTED_MODULE_0_jquery__);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_1_moment__ = __webpack_require__(0);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_1_moment___default = __webpack_require__.n(__WEBPACK_IMPORTED_MODULE_1_moment__);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_2_moment_locale_hu__ = __webpack_require__(1);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_2_moment_locale_hu___default = __webpack_require__.n(__WEBPACK_IMPORTED_MODULE_2_moment_locale_hu__);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_3__HubConnection__ = __webpack_require__(117);




__WEBPACK_IMPORTED_MODULE_0_jquery__(document).ready(() => {
    __WEBPACK_IMPORTED_MODULE_1_moment__["locale"]('hu');
    let hubConnection = new __WEBPACK_IMPORTED_MODULE_3__HubConnection__["a" /* HubConnection */]('ws://localhost:5000');
    let addMessage = (message) => {
        __WEBPACK_IMPORTED_MODULE_0_jquery__("#messages").append(`<p>${__WEBPACK_IMPORTED_MODULE_1_moment__().format('LL LTS')}: ${message}</p>`);
    };
    let displayTime = (currentTime) => {
        __WEBPACK_IMPORTED_MODULE_0_jquery__("#currentTime span").text('Current server time is: ' + __WEBPACK_IMPORTED_MODULE_1_moment__(currentTime).format('LL LTS'));
    };
    hubConnection.onClosed = (error) => {
        if (error) {
            addMessage(error.message);
        }
        else {
            addMessage('Connection to the hub closed cleanly.');
        }
    };
    __WEBPACK_IMPORTED_MODULE_0_jquery__("#connectButton").click((e) => {
        e.preventDefault();
        hubConnection.connect()
            .then(() => {
            addMessage('Connected to the hub!');
            hubConnection.subscribe("timer", displayTime);
        })
            .catch((reason) => {
            addMessage('Connection refused because reason: ' + reason.data);
        });
    });
    __WEBPACK_IMPORTED_MODULE_0_jquery__("#closeButton").click((e) => {
        e.preventDefault();
        hubConnection.unsubscribe("timer");
        hubConnection.disconnect();
    });
});


/***/ })

},[120]);