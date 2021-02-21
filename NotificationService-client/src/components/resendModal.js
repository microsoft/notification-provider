// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import {Dialog,DialogFooter,DialogType,PrimaryButton,DefaultButton} from 'office-ui-fabric-react';
import {resendEmailService} from "../services";
import React, {useState} from 'react';

export default function ResendModal (props) {
    var selectedItems = props.selectedItem?props.selectedItem:[];
    var notificationIds = [];
    const [resendStatus, setResendStatus] = useState("");
    selectedItems.forEach(e=>notificationIds.push(e.notificationId));
    const resendEmail = (e) => {
        if(notificationIds.length>0)
            resendEmailService(notificationIds).then((res)=>{
                setResendStatus("Success");
            }).catch(e=>
                setResendStatus("error"));
    }
    return (
        <Dialog 
            hidden={props.hideDialog} 
            onDismiss={props.toggleHideDialog}
            dialogContentProps={{
                    type: DialogType.largeHeader,
                    title: 'Resend Emails',
                    subText: 'The following notification ids will be re-queued for resend:'
        }}>
            {notificationIds?.join(" , ")}
            {resendStatus}
        <DialogFooter>
            <PrimaryButton onClick={resendEmail} text="Send" />
            <DefaultButton onClick={props.toggleHideDialog} text="Don't send" />
        </DialogFooter>
    </Dialog>
    )
}