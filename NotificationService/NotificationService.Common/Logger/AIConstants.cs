// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Common.Logger
{
    /// <summary>
    /// AI logging constants class.
    /// </summary>
    public static class AIConstants
    {
        /// <summary>
        /// Event code literal.
        /// </summary>
        public const string EventCodeKeyName = "EventCode";

        /// <summary>
        /// Duration.
        /// </summary>
        public const string EnvironmentName = "EnvironmentSetting:EnvironmentType";

        /// <summary>
        /// ComponentId.
        /// </summary>
        public const string ComponentIdConfigName = "ApplicationInsights:ComponentId";

        /// <summary>
        /// ComponentName.
        /// </summary>
        public const string ComponentNameConfigName = "ApplicationInsights:ComponentName";

        /// <summary>
        /// Service.
        /// </summary>
        public const string ServiceConfigName = "ApplicationInsights:Service";

        /// <summary>
        /// ServiceLine.
        /// </summary>
        public const string ServiceLineConfigName = "ApplicationInsights:ServiceLine";

        /// <summary>
        /// ServiceOffering.
        /// </summary>
        public const string ServiceOfferingConfigName = "ApplicationInsights:ServiceOffering";

        /// <summary>
        /// Application literal.
        /// </summary>
        public const string Application = "Application";

        /// <summary>
        /// NotificationIds literal.
        /// </summary>
        public const string NotificationIds = "NotificationIds";

        /// <summary>
        /// Duration literal.
        /// </summary>
        public const string Duration = "Duration";

        /// <summary>
        /// EmailNotificationCount literal.
        /// </summary>
        public const string EmailNotificationCount = "EmailNotificationCount";

        /// <summary>
        /// MeetingNotificationCount literal.
        /// </summary>
        public const string MeetingNotificationCount = "MeetingNotificationCount";

        /// <summary>
        /// Result literal.
        /// </summary>
        public const string Result = "Result";

        /// <summary>
        /// NotificationType literal.
        /// </summary>
        public const string NotificationType = "NotificationType";

        /// <summary>
        /// CorrelationId literal.
        /// </summary>
        public const string CorrelationId = "X-CorrelationId";

        /// <summary>
        /// The EmailAccount literal.
        /// </summary>
        public const string EmailAccount = "EmailAccount";

        /// <summary>
        /// The Email template name.
        /// </summary>
        public const string MailTemplateName = "MailTemplateName";

        /// <summary>
        /// The Email template name.
        /// </summary>
        public const string MailTemplateId = "MailTemplateId";

        /// <summary>
        /// DateRange for resend.
        /// </summary>
        public const string ResendDateRange = "ResendDateRange";

        /// <summary>
        /// Custom Event MailBox Exhausted.
        /// </summary>
        public const string CustomEventMailBoxExhausted = "Mail Box Limit Exhausted";

        /// <summary>
        /// Custom Event for send email notificaiton failure.
        /// </summary>
        public const string CustomEventMailSendFailed = "Mail Send Failed";

        /// <summary>
        /// Custom Event for send invite notificaiton failure.
        /// </summary>
        public const string CustomEventInviteSendFailed = "Meeting Invite Send Failed";
    }
}
