import {instance} from "./auth/authProvider";
import {config} from "./configuration/config"

export  function getMailHistory(token, filter) {
    var reqBody = prepareReqBody(token,filter);
    return instance.post(config.serviceEndpoints.mailHistoryEndpoint,reqBody,{headers:{Authorization:""}});
};

export  function resendEmailService(notificationIds) {
    return instance.post(config.serviceEndpoints.resendEmailEndpoint, notificationIds)
};

export function viewMailBody(notificationId) {
    return instance.get(config.serviceEndpoints.viewMailBodyEndpoint+notificationId);
}

const prepareReqBody = (token,filter) => {
    var notificationIdsFilter = filter?.filter(e=>(e.key===0))?.map(e=>e.value)?.[0];
    var notificationStatusFilter = (filter?.filter(e=>(e.key===1))?.map(e=>e.value.map(e=>parseInt(e))))?.[0];
    var reqBody = {
        "applicationFilter":["LoadTest"],
        "notificationIdsFilter": notificationIdsFilter?.length>0?notificationIdsFilter:[],
        "notificationStatusFilter":notificationStatusFilter?.length>0?notificationStatusFilter:[],
          take:100,
          token:token
    }
    return reqBody;
}