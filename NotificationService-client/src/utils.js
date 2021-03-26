// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

export  function formatString(source, params) {
    var str = source;
    for(const key in params){
        str = str.replace("{"+key+"}", params[key]);
    }
    return str;
}