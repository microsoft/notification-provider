import { UserAgentApplication } from 'msal';
import axios from 'axios';
import {config} from "../configuration/config";

export const instance = axios.create();

const MsalObj = new UserAgentApplication({
    auth: {
        clientId: config.msalConfig?.clientId,
        authority: config.msalConfig?.authority,
        postLogoutRedirectUri: config.msalConfig?.postLogoutRedirectUri,
        redirectUri: config.msalConfig?.redirectUri
    },
    cache: {
        cacheLocation: 'sessionStorage',
        storeAuthStateInCookie: false
    }
});

const loginRequest = {
    scopes: config.msalConfig.scopes
};

const idTokenLoginRequest = {
    scopes: [config.msalConfig.clientId]
};

MsalObj.handleRedirectCallback(authRedirectCallback);

function authRedirectCallback(error, response) {
    if (error) {
        return;
    } else {
        configureUserDetails();
    }
}

export function configureUserDetails() {
    const account = getAccountDetails();
    if (account && account.idToken) {
        config.userProfile.fullName = account.name;
        config.userProfile.objectId = account.accountIdentifier;
        config.userProfile.email = account.userName;
    }
}

export function signIn() {
    MsalObj.loginRedirect(loginRequest);
}

export function signOut() {
    MsalObj.logout();
}

export function getAccountDetails() {
    return MsalObj.getAccount();
}

export async function getToken() {
    try {
    const response = await MsalObj.acquireTokenSilent(idTokenLoginRequest);
    if (response && response.idToken && response.idToken.rawIdToken) {
        sessionStorage.setItem('msal.idtoken', response.idToken.rawIdToken);
    } else {
        signIn();
    }
    Promise.resolve();
    }
    catch (e) {
        signIn();
    }
    
}

instance.interceptors.request.use(config1 => {
        if (sessionStorage.getItem('msal.idtoken')) {
            config1.headers.Authorization = `Bearer ${sessionStorage.getItem('msal.idtoken')}`;
        }
        else {
            getToken();
        }
        return Promise.resolve(config1);
    });

instance.interceptors.response.use(response => {
   return Promise.resolve(response);
},error => {
    signIn();
});