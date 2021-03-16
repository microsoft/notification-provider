// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import {instance} from "./auth/authProvider";
import {config} from "./configuration/config";
import {formatString} from "./utils";

export  function getMailHistory(applicationName, token, filter) {
    var reqBody = prepareReqBody(applicationName,token,filter);
    return instance.post(config.serviceEndpoints.mailHistoryEndpoint,reqBody,{headers:{Authorization:""}});
};

export  function resendEmailService(applicationName, notificationIds) {
    const params = {"applicationName": applicationName};
    const url = formatString(config.serviceEndpoints.resendEmailEndpoint, params);
    return instance.post(url, notificationIds)
};

export function viewMailBody(applicationName, notificationId) {
    const params = {"applicationName":applicationName, "notificationId":notificationId}
    const url = formatString(config.serviceEndpoints.viewMailBodyEndpoint, params);
    return instance.get(url);
}

const prepareReqBody = (applicationName,token,filter) => {
    var notificationIdsFilter = filter?.filter(e=>(e.key===0))?.map(e=>e.value)?.[0];
    var notificationStatusFilter = (filter?.filter(e=>(e.key===1))?.map(e=>e.value.map(e=>parseInt(e))))?.[0];
    var appName = [applicationName];
    var reqBody = {
        "applicationFilter":appName,
        "notificationIdsFilter": notificationIdsFilter?.length>0?notificationIdsFilter:[],
        "notificationStatusFilter":notificationStatusFilter?.length>0?notificationStatusFilter:[],
          take:100,
          token:token
    }
    return reqBody;
}