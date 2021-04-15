// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import {Dialog,DialogFooter,DialogType,PrimaryButton,DefaultButton, ProgressIndicator, mergeStyleSets} from 'office-ui-fabric-react';
import {resendEmailService, resendMeetingService} from "../services";
import React, {useState} from 'react';

const resendStyles = new mergeStyleSets({
    greenText:{
        color:'green'
    },
    redText:{
        color:'red'
    }
});

export default function ResendModal (props) {
    var selectedItems = props.selectedItem?props.selectedItem:[];
    var resendTitle = props.title;
    var notificationType = props.notificationType;
    var notificationIds = [];
    const [loader, setLoader] = useState(false);
    const [resendStatus, setResendStatus] = useState("");
    selectedItems.forEach(e=>notificationIds.push(e.notificationId));
    const applicationName = props.application;
    const toggleResendDialog = () => {
        props.toggleHideDialog();
        setResendStatus("");
    };

    const handleSuccess = (res) => {
        setResendStatus(res ? "Success": "Failed");
        setLoader(false);
    };

    const handleException = (error) => {
        setResendStatus("Failed");
        setLoader(false);
    };

    const resendEmail = (e) => {
        setResendStatus("");
        if(notificationIds.length > 0){
            setLoader(true);
            if(notificationType === 'Mail')
            {
                resendEmailService(applicationName, notificationIds).then((res)=>{
                    handleSuccess(res);
                }).catch(e=> {
                    handleException();
                });
            }
            else
            {
                resendMeetingService(applicationName, notificationIds).then((res)=>{
                    handleSuccess(res);
                }).catch(e=> {
                    handleException();
                });
            }
        }
    };

    return (
        <Dialog 
            hidden={props.hideDialog} 
            onDismiss={toggleResendDialog}
            dialogContentProps={{
                    type: DialogType.largeHeader,
                    title: resendTitle,
                    subText: 'The following notification ids will be re-queued for resend:'
            }}>
            {notificationIds?.join(" , ")}
            <br/>
            <br/>
            {resendStatus.length >0 ? resendStatus === 'Success' ? <div className={resendStyles.greenText}>Resend Success.</div> : <div className={resendStyles.redText}>Resend Failed.</div> : ""}
            {loader === true ? <ProgressIndicator/> : ""}
            <DialogFooter>
                <PrimaryButton onClick={resendEmail} text="Send" />
                <DefaultButton onClick={toggleResendDialog} text="Close" />
            </DialogFooter>
        </Dialog>
    )
}