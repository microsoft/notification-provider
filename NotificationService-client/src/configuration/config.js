// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

export var config = {
    msalConfig : {
        "clientId": window.app.env.clientID,
        "authority":"https://login.microsoftonline.com/72f988bf-86f1-41af-91ab-2d7cd011db47",
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
    applicationName: window.app.env.defaultApplication
}