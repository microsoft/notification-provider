// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Common
{
    /// <summary>
    /// Common constants used across the solution.
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// Character used to split mutliple values in a string.
        /// </summary>
        public const char SplitCharacter = ';';

        /// <summary>
        /// Content Type of the body of email notifications.
        /// </summary>
        public const string EmailBodyContentType = "HTML";

        /// <summary>
        /// Claim Type for the Audience Claim in JWT token.
        /// </summary>
        public const string AudienceClaimType = "aud";

        /// <summary>
        /// Claim Type for the Audience Claim in JWT token.
        /// </summary>
        public const string ExpiryClaimType = "exp";

        /// <summary>
        /// Name of the query string parameter to get Application Name.
        /// </summary>
        public const string ApplicationNameQueryParameter = "applicationName";

        /// <summary>
        /// Name of Authorization policy to validate if Application Name is valid.
        /// </summary>
        public const string AppNameAuthorizePolicy = "AppNameAuthorize";

        /// <summary>
        /// Name of Authorization policy to validate if the caller is valid audience of the Application.
        /// </summary>
        public const string AppAudienceAuthorizePolicy = "AppAudienceAuthorize";

        /// <summary>
        /// Authentication scheme to validate the input token.
        /// </summary>
        public const string BearerAuthenticationScheme = "Bearer";

        /// <summary>
        /// Name of Authorization Header in Request.
        /// </summary>
        public const string AuthorizationHeaderName = "Authorization";

        /// <summary>
        /// Content type of the body/response of an Http Request.
        /// </summary>
        public const string JsonMIMEType = "application/json";

        /// <summary>
        /// Method name for POST API request.
        /// </summary>
        public const string POSTHttpVerb = "POST";

        /// <summary>
        /// Seconds to wait between attempts at polling the Azure KeyVault for changes in configuration.
        /// </summary>
        public const string KeyVaultConfigRefreshDurationSeconds = "KeyVaultConfigRefreshDurationSeconds";

        /// <summary>
        /// Purpose string used to encrypt/decrypt data using Data Protection.
        /// </summary>
        public const string EncryptionPurposeString = "NotificationService";

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
        /// BatchId literal.
        /// </summary>
        public const string BatchId = "BatchId";

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
        /// The application accounts.
        /// </summary>
        public const string ApplicationAccounts = "ApplicationAccounts";

        /// <summary>
        /// The EmailAccount literal.
        /// </summary>
        public const string EmailAccount = "EmailAccount";

        /// <summary>
        /// StorageType.
        /// </summary>
        public const string StorageType = "StorageType";

        /// <summary>
        /// The Email template name.
        /// </summary>
        public const string MailTemplateName = "MailTemplateName";

        /// <summary>
        /// NotificationProviderType.
        /// </summary>
        public const string NotificationProviderType = "NotificationProviderType";

        /// <summary>
        /// The notification encryption key.
        /// </summary>
        public const string NotificationEncryptionKey = "NotificationEncryptionKey";

        /// <summary>
        /// The notification encryption intial vector.
        /// </summary>
        public const string NotificationEncryptionIntialVector = "NotificationEncryptionIntialVector";

        /// <summary>
        /// The meetingnotificationsqueue.
        /// </summary>
        public const string Notificationsqueue = "notifications-queue";

        /// <summary>
        /// A formatter for Meeting invite Start and End Dates for Graph api base meeting invite.
        /// </summary>
        public const string GraphMeetingInviteDateTimeFormatter = "yyyy-MM-ddThh:mm:ss";

        /// <summary>
        /// A formatter for Meeting invite RecurrenceRangeDate for Graph api base meeting invite.
        /// </summary>
        public const string GraphMeetingInviteRecurrenceRangeDateFormatter = "yyyy-MM-dd";
    }
}
