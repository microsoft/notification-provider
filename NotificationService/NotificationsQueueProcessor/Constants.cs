// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationsQueueProcessor
{
    /// <summary>
    /// Constants used by application.
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// Content type of the body/response of an Http Request.
        /// </summary>
        public const string JsonMIMEType = "application/json";

        /// <summary>
        /// Email notification type.
        /// </summary>
        public const string EmailNotificationType = "email";

        /// <summary>
        /// Meeting notification type.
        /// </summary>
        public const string MeetingNotificationType = "meetinginvite";

        /// <summary>
        /// Authority variable name.
        /// </summary>
        public const string Authority = "Authority";

        /// <summary>
        /// Client Id  variable name.
        /// </summary>
        public const string ClientId = "ClientId";

        /// <summary>
        /// Client Secret variable name.
        /// </summary>
        public const string ClientSecret = "ClientSecret";

        /// <summary>
        /// Notification service endpoint variable name.
        /// </summary>
        public const string NotificationServiceEndpoint = "NotificationServiceEndpoint";

        /// <summary>
        /// TimeOut limit in seconds for HTTP request.
        /// </summary>
        public const string HttpTimeOutInSec = "HttpTimeOutInSeconds";

        /// <summary>
        /// StorageType.
        /// </summary>
        public const string StorageType = "StorageType";

        /// <summary>
        /// Seconds to wait between attempts at polling the Azure KeyVault for changes in configuration.
        /// </summary>
        public const string KeyVaultConfigRefreshDurationSeconds = "KeyVaultConfigRefreshDurationSeconds";
        /// <summary>
        /// ManagedIdentity Id
        /// </summary>
        public const string ManagedIdentity = "ManagedIdentity";
    }
}
