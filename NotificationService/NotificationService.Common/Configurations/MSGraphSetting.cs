// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Common
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// MS Graph Configuration Settings.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class MSGraphSetting
    {
        /// <summary>
        /// Gets or sets Client ID.
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// Gets or sets Client Credential.
        /// </summary>
        public string ClientCredential { get; set; }

        /// <summary>
        /// Gets or sets User Assertion Type.
        /// </summary>
        public string UserAssertionType { get; set; }

        /// <summary>
        /// Gets or sets Authority.
        /// </summary>
        public string Authority { get; set; }

        /// <summary>
        /// Gets or sets Tenant ID.
        /// </summary>
        public string TenantId { get; set; }

        /// <summary>
        /// Gets or sets Graph Resource ID.
        /// </summary>
        public string GraphResourceId { get; set; }

        /// <summary>
        /// Gets or sets Graph API Version.
        /// </summary>
        public string GraphAPIVersion { get; set; }

        /// <summary>
        /// Gets or sets the Graph url to send email.
        /// </summary>
        public string SendMailUrl { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether graph requests to be batched or not.
        /// </summary>
        public bool EnableBatching { get; set; }

        /// <summary>
        /// Gets or sets the Graph url to send Batch Request.
        /// </summary>
        public string BatchRequestUrl { get; set; }

        /// <summary>
        /// Gets or sets the number of requests that can be accomodated in single batch request.
        /// </summary>
        public int BatchRequestLimit { get; set; }

        /// <summary>
        /// Gets or sets the Graph url to send meeting invite.
        /// </summary>
        public string SendInviteUrl { get; set; }

        /// <summary>
        /// Gets or sets the Graph Base Url.
        /// </summary>
        public string BaseUrl { get; set; }
    }
}
