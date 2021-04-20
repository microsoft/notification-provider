// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { toast, Zoom } from 'react-toastify';

export  function formatString(source, params) {
    var str = source;
    for(const key in params){
        str = str.replace("{"+key+"}", params[key]);
    }
    return str;
}

export const copyToClipboard = (item, index, event) => {
    const selectedValue = event?.path[0]?.innerText;
    if(selectedValue){
        navigator.clipboard.writeText(selectedValue);
        toast.info('Copied to clipboard!', {
            position: toast.POSITION.TOP_CENTER,
            autoClose: 2000,
            hideProgressBar: true,
            closeOnClick: true,
            pauseOnHover: true,
            draggable: true,
            progress: undefined,
            closeButton: false,
            transition: Zoom
            });
    }
}