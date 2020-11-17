// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Common.Configurations
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Text;

    /// <summary>
    /// MailSettings Configuration Settings.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class MailSettings
    {
        /// <summary>
        /// Gets or sets Application Name.
        /// </summary>
        public string ApplicationName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Mails should be actually sent or not.
        /// </summary>
        public bool MailOn { get; set; } = true;

        /// <summary>
        /// Gets or sets Email address which shall override the to address of recipients.
        /// </summary>
        public string ToOverride { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether SendForReal is true or false, If false ToOverride would be used to override the recipient address.
        /// </summary>
        public bool SendForReal { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether gets or sets SaveToSent.
        /// </summary>
        public bool SaveToSent { get; set; } = true;
    }
}
