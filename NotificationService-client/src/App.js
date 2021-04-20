// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import {useState} from 'react';
import {CoherenceHeader} from "@cseo/controls";
import MailHistory from "./components/mailHistory";
import { Customizer } from 'office-ui-fabric-react';
import { CoherenceCustomizations } from '@cseo/styles';
import { useMsal } from "./auth/authProvider";
import { BrowserRouter, Redirect, Route, Switch } from 'react-router-dom';
import NotificationNav from './components/left-nav';
import { SetOnsearchDetailsView  } from '@cseo/controls';
import MeetingHistory from './components/meetingHistory';

function App() {
  
  const [isNavCollapsed, setIsNavCollapsed] = useState(false);
  const _onNavCollapsed = (isCollapsed) => {
     setIsNavCollapsed(isCollapsed);
  };

  const _goBackClicked = () => {
    SetOnsearchDetailsView(false);
  }
 
  const { isAuthenticated, user, signOut } = useMsal()
  
  return (
    <Customizer {...CoherenceCustomizations}>
    {isAuthenticated ?
    (<>  <CoherenceHeader
         headerLabel={'header'}
         appNameSettings={{
             label: 'Notification Service'
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
         <BrowserRouter>
          <>
            <NotificationNav onNavCollapsed={_onNavCollapsed} onNavItemClicked={_goBackClicked} />
            <main id='main' tabIndex={-1}>
                            {
                                <Switch>
                                    <Redirect exact from="/" to="/mailHistory" />
                                    <Route exact path="/mailHistory" render={() => <MailHistory isNavCollapsed={isNavCollapsed} />} />
                                    <Route exact path="/meetingHistory" render={() => <MeetingHistory isNavCollapsed={isNavCollapsed} />} />
                                </Switch>
                            }
            </main>
          </>
         </BrowserRouter>
    </> ) : ''}
  </Customizer>);
}

export default App;
