// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

export var config = {
    serviceEndpoints :{
        mailHistoryEndpoint: window.app.env.serviceEndpoints.mailHistoryEndpoint,
        viewMailBodyEndpoint: window.app.env.serviceEndpoints.viewMailBodyEndpoint,
        resendEmailEndpoint: window.app.env.serviceEndpoints.resendEmailEndpoint,
        applicationsEndpoint: window.app.env.serviceEndpoints.applicationsEndpoint,
        meetingHistoryEndpoint: window.app.env.serviceEndpoints.meetingHistoryEndpoint,
        viewMeetingBodyEndpoint: window.app.env.serviceEndpoints.viewMeetingBodyEndpoint, 
        resendMeetingEndpoint:window.app.env.serviceEndpoints.resendMeetingEndpoint
    },
    recordsPerPage: window.app.env.recordsPerPage
}