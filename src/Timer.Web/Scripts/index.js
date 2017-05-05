import * as $ from 'jquery';
import * as moment from 'moment';
import * as Oidc from 'oidc-client';
import 'moment/locale/hu';
import { HubConnection } from './HubConnection';
$(document).ready(() => {
    moment.locale('hu');
    let hubConnection = new HubConnection('ws://localhost:5000');
    let config = {
        authority: 'http://localhost:5001',
        client_id: 'javascript-client',
        redirect_uri: 'http://localhost:5000/callback.html',
        response_type: 'id_token token',
        scope: 'openid profile websocket_api.subscribe',
        post_logout_redirect_url: 'http://localhost:5000/index.html'
    };
    let mgr = new Oidc.UserManager(config);
    mgr.getUser().then(user => {
        if (user) {
            console.log('Logged in!');
            hubConnection.setAuthToken(user.access_token);
        }
        else {
            console.log('NOT Logged in!');
        }
    });
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
    $("#loginButton").click((e) => {
        e.preventDefault();
        mgr.signinRedirect();
    });
    $("#logoutButton").click((e) => {
        e.preventDefault();
        mgr.signoutRedirect();
    });
});
//# sourceMappingURL=index.js.map