// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { WindowUtils } from "msal";

export var config = {
    msalConfig : {
        "clientId": window.app.env.clientID,
        "authority": window.app.env.authority,
        "postLogoutRedirectUri":`${window.location.origin}`,
        "redirectUri":`${window.location.origin}`,
        "scopes":""
    },
    userProfile : {
        "fullName" : "",
        "objectId" : "",
        "email" : ""
    },
    serviceEndpoints :{
        mailHistoryEndpoint: window.app.env.serviceEndpoints.mailHistoryEndpoint,
        viewMailBodyEndpoint: window.app.env.serviceEndpoints.viewMailBodyEndpoint,
        resendEmailEndpoint: window.app.env.serviceEndpoints.resendEmailEndpoint
    },
    applicationName: window.app.env.defaultApplication,
    recordsPerPage: window.app.env.recordsPerPage
}