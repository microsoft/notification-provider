// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React from 'react';
import ReactDOM from 'react-dom';
import App from './App';
import { initializeIcons } from 'office-ui-fabric-react';
import {MsalProvider} from './auth/authProvider';
import { msalConfig, loginRequest } from './configuration/auth-config' ;

initializeIcons();
ReactDOM.render(
  <MsalProvider
  config={msalConfig}
  scopes={loginRequest}
  >
    <App />
  </MsalProvider>,
  document.getElementById('root')
);
