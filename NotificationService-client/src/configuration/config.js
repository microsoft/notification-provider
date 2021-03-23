// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

export var config = {
    serviceEndpoints :{
        mailHistoryEndpoint: window.app.env.serviceEndpoints.mailHistoryEndpoint,
        viewMailBodyEndpoint: window.app.env.serviceEndpoints.viewMailBodyEndpoint,
        resendEmailEndpoint: window.app.env.serviceEndpoints.resendEmailEndpoint,
        applicationsEndpoint: window.app.env.serviceEndpoints.applicationsEndpoint
    },
    recordsPerPage: window.app.env.recordsPerPage
}