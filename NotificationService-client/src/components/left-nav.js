import * as React from 'react';
import { CoherenceNav } from '@cseo/controls';
import { withRouter } from 'react-router-dom';

class NotificationNavBase extends React.Component {
    render() {
        return (
            <CoherenceNav
                appName={'Coherence demo site'}
                groups={[
                    {
                        key: 'Left Nav Menu',
                        links: [
                            {
                                name: 'Mail History',
                                key: 'mailHistoryKey',
                                ariaLabel: 'Email History',
                                icon: 'Mail',
                                'data-id': 'automation_id_22231',
                                onClick: () => {
                                    this.props.history.push('/mailHistory');
                                    this.props.onNavItemClicked();
                                },
                                isSelected: window.location.href === window.location.protocol + '//' + window.location.host + '/mailHistory' ? true: false
                            },
                            {
                                name: 'Meeting History',
                                key: 'meetingHistoryKey',
                                ariaLabel: 'Meeting History',
                                icon: 'Event',
                                'data-id': 'automation_id_22232',
                                onClick: () => {
                                    this.props.history.push('/meetingHistory');
                                    this.props.onNavItemClicked();
                                },
                                isSelected: 
                                    window.location.href === 
                                    window.location.protocol + 
                                    '//' + 
                                    window.location.host + 
                                    '/meetingHistory' 
                                    ? true 
                                    : false
                            },
                            /*
                            {
                                name: 'Templates',
                                key: 'templatesKey',
                                ariaLabel: 'Templates',
                                icon: 'FileTemplate',
                                target: '_blank',
                                onClick: () => {
                                    this.props.history.push('/templates');
                                    this.props.onNavItemClicked();
                                },
                                isSelected:
                                    window.location.href ===
                                        window.location.protocol +
                                        '//' +
                                        window.location.host +
                                        '/templates'
                                        ? true
                                        : false
                            },
                            */
                            
                        ]
                    }
                ]}
                onNavCollapsed={this.props.onNavCollapsed}
            />
        );
    }
}

export default withRouter(NotificationNavBase);
