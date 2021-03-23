// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import {CoherenceHeader} from "@cseo/controls";
import MailHistory from "./components/mailHistory";
import { useMsal } from "./auth/authProvider";

function App() {
  const { loading, isAuthenticated, user, signOut } = useMsal()
  if (loading) {
    return <div>Loading...</div>;
  }
  return (<>
    {isAuthenticated ?
    (<main>
        <CoherenceHeader
         headerLabel={'header'}
         appNameSettings={{
             label: 'Email History'
         }}
         farRightSettings={{
          profileSettings: {
            fullName: user?.name,
            emailAddress: user?.username,
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
