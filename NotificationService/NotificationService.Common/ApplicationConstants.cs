// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Common
{
    /// <summary>
    /// Common constants used across the solution.
    /// </summary>
    public static class ApplicationConstants
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
        //public const string AudienceClaimType = "aud";

        /// <summary>
        /// Claim Type for the Audience Claim in JWT token.
        /// </summary>
        public const string AppIdClaimType = "appid";

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

        /// <summary>s
        /// Name of Authorization policy to validate if the caller is valid audience of the Application.
        /// </summary>
        public const string AppIdAuthorizePolicy = "AppIdAuthorize";

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
        /// EmailNotificationsFolderName.
        /// </summary>
        public const string EmailNotificationsFolderName = "EmailNotifications";

        /// <summary>
        /// MeetingNotificationsFolderName.
        /// </summary>
        public const string MeetingNotificationsFolderName = "MeetingNotifications";

        /// <summary>
        /// A formatter for Meeting invite Start and End Dates for Graph api base meeting invite.
        /// </summary>
        public const string GraphMeetingInviteDateTimeFormatter = "yyyy-MM-ddThh:mm:ss";

        /// <summary>
        /// A formatter for Meeting invite RecurrenceRangeDate for Graph api base meeting invite.
        /// </summary>
        public const string GraphMeetingInviteRecurrenceRangeDateFormatter = "yyyy-MM-dd";

        /// <summary>
        /// A constant for ResendDateRange logging.
        /// </summary>
        public const string ResendDateRange = "ResendDateRange";

        /// <summary>
        /// A constact for appId in AADV2 claim type.
        /// </summary>
        public const string AppIdV2ClaimType = "azp";

        /// <summary>
        /// A constant used to insert these many items at once in a single batch to storage.
        /// The azure storage has a limitation of accepting only 100 items in a single batch, so keeping the count to 100.
        /// </summary>
        public const int BatchSizeToStore = 100;
    }
}
