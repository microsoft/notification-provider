// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import {CoherenceHeader} from "@cseo/controls";
import MailHistory from "./components/mailHistory";
import {signOut,getToken} from "./auth/authProvider";
import {config} from "./configuration/config";
import { useEffect,useState } from "react";
function App() {
  const [isLoggedIn, setIsLoggedIn] = useState(false);
  useEffect(()=>{
   getToken().then(()=>{
     if(sessionStorage.getItem('msal.idtoken')!==null) 
      setIsLoggedIn(true);
   });
  },[]); 
  return (<>
    {isLoggedIn===true ?
    (<main>
        <CoherenceHeader
         headerLabel={'header'}
         appNameSettings={{
             label: 'Email History'
         }}
         farRightSettings={{
          profileSettings: {
            fullName: config.userProfile.fullName,
            emailAddress: config.userProfile.email,
            imageUrl: undefined,
            logOutLink: '#',
            onLogOut: () => signOut()
          }
         }}/> 
         <MailHistory/>
    </main>): ''}
  </>);
}

export default App;
