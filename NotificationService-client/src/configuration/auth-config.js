// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

export const msalConfig = {
    auth: {
        clientId: window.app.env.clientID,
        authority: window.app.env.authority,
        postLogoutRedirectUri: `${window.location.origin}`,
        redirectUri: `${window.location.origin}`
    },
    cache: {
        cacheLocation: 'sessionStorage',
        storeAuthStateInCookie: true
    }
};

export const loginRequest = {
    scopes: window.app.env.apiScope,
    forceRefresh: false
};

export const idTokenLoginRequest = {
    scopes: [window.app.env.clientID],
    forceRefresh: false
};

