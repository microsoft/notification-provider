// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Common.Configurations
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// SMTP Configuration Settings.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class SMTPSetting
    {
        /// <summary>
        /// Gets or sets SmtpUrl.
        /// </summary>
        public string SmtpUrl { get; set; }

        /// <summary>
        /// Gets or sets SmtpPort.
        /// </summary>
        public int SmtpPort { get; set; }

        /// <summary>
        /// Gets or sets SmtpDomain.
        /// </summary>
        public string SmtpDomain { get; set; }
    }
}
