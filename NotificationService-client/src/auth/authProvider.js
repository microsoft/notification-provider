// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import axios from 'axios';
import {loginRequest} from '../configuration/auth-config';
import { PublicClientApplication, InteractionRequiredAuthError } from "@azure/msal-browser";
import React, { useState, useContext, useEffect } from "react";

export const instance = axios.create();
export const MsalContext = React.createContext();
export const useMsal = () => useContext(MsalContext);
export const MsalProvider = ({
    children,
    config
}) => {
    const [isAuthenticated, setIsAuthenticated] = useState(false);
    const [user, setUser] = useState();
    instance.interceptors.request.use(config1 => {    
        if (sessionStorage.getItem('msal.idtoken')) {
            config1.headers.Authorization = `Bearer ${sessionStorage.getItem('msal.idtoken')}`;
        }
        else {
            signIn();
        }
        return Promise.resolve(config1);       
    });

    instance.interceptors.response.use(response => {
        return Promise.resolve(response);
    },error => {
        //signIn();
    });

    let accountId = "";
    const myMSALObj = new PublicClientApplication(config); 
    useEffect(() => {
        myMSALObj.handleRedirectPromise().then((resp) => {
            if (resp !== null) {
                console.log("AccessToken : " + resp.accessToken);
                sessionStorage.setItem('msal.idtoken', resp.idToken);
                accountId = resp.account.homeAccountId;
                setIsAuthenticated(true);
                setUser(resp.account);
                myMSALObj.setActiveAccount(resp.account);
            } else {
                const currentAccount = GetAccount();
                if(!currentAccount){
                    signIn();
                }
            }
            Promise.resolve(resp);
        }).catch(error => {
            console.log("authentication error : " + error);
        });
    },[]);
    
    const signIn = async () => {
        myMSALObj.loginRedirect(loginRequest);  
    };

    const signOut = async() => {
        const logoutRequest = {
            account: myMSALObj.getAccountByHomeId(accountId)
        };
        myMSALObj.logout(logoutRequest);
    };

    const GetAccount = () => {
        const account = myMSALObj.getActiveAccount();
        if (account) {
            accountId = account.homeAccountId;
        }
        return account;
    };

    const getToken = async () => {
        const account = GetAccount();
        const request = {...loginRequest, account: account};
        return await myMSALObj.acquireTokenSilent(request).catch(async (error) => {
        console.log("silent token acquisition fails. :: " + error);
            if(error instanceof InteractionRequiredAuthError){
                console.log("acquiring token using redirect");
                return myMSALObj.acquireTokenRedirect({...loginRequest});
            }
        });
    };

    return (
        <MsalContext.Provider
            value={{
            isAuthenticated,
            user,
            signIn,
            signOut,
            getToken
            }}
        >
            {children}
        </MsalContext.Provider>
    );
};