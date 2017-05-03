import * as $ from 'jquery';
import * as moment from 'moment';
import 'moment/locale/hu';
import { HubConnection } from './HubConnection';
$(document).ready(() => {
    moment.locale('hu');
    let hubConnection = new HubConnection('ws://localhost:5000');
    let addMessage = (message) => {
        $("#messages").append(`<p>${moment().format('LL LTS')}: ${message}</p>`);
    };
    let displayTime = (currentTime) => {
        $("#currentTime span").text('Current server time is: ' + moment(currentTime).format('LL LTS'));
    };
    hubConnection.onClosed = (error) => {
        if (error) {
            addMessage(error.message);
        }
        else {
            addMessage('Connection to the hub closed cleanly.');
        }
    };
    $("#connectButton").click((e) => {
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
    $("#closeButton").click((e) => {
        e.preventDefault();
        hubConnection.unsubscribe("timer");
        hubConnection.disconnect();
    });
});
//# sourceMappingURL=index.js.map