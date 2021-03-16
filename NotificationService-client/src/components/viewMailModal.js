// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import {Dialog,DialogFooter,DialogType,DefaultButton,ProgressIndicator} from 'office-ui-fabric-react';

export default function ViewMailModal (props) {
    const {mailBody} = props;
    return (
        <Dialog 
            hidden={props.hideDialog} 
            onDismiss={props.toggleMailDialog}
            dialogContentProps={{
                    type: DialogType.largeHeader,
                    title: 'Mail Body'
            }}>
            {props.showLoader===true?<ProgressIndicator/>:""}
            <div dangerouslySetInnerHTML={{__html: mailBody}} />
            <DialogFooter>
                <DefaultButton onClick={props.toggleMailDialog} text="Close" />
            </DialogFooter>
    </Dialog>)
}