import * as $ from 'jquery';
import * as Oidc from 'oidc-client';
$(document).ready(() => {
    new Oidc.UserManager({}).signinRedirectCallback().then(() => {
        window.location.href = "index.html";
    }).catch(e => {
        console.error(e);
    });
});
//# sourceMappingURL=callback.js.map