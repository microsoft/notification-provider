import {CoherenceHeader} from "@cseo/controls";
import MailHistory from "./components/mailHistory";
import {signOut} from "./auth/authProvider";
import {config} from "./configuration/config";
function App() {
  return (
    <div>
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
    </div>
  );
}

export default App;
