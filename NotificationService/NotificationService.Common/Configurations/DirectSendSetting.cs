// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Common.Configurations
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Direct Send Configuration Settings.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class DirectSendSetting
    {
        /// <summary>
        /// Gets or sets FromAddressDisplayName.
        /// </summary>
        public string FromAddressDisplayName { get; set; }

        /// <summary>
        /// Gets or sets FromAddress.
        /// </summary>
        public string FromAddress { get; set; }

        /// <summary>
        /// Gets or sets SmtpPort.
        /// </summary>
        public string SmtpPort { get; set; }

        /// <summary>
        /// Gets or sets SmtpServer.
        /// </summary>
        public string SmtpServer { get; set; }
    }
}
