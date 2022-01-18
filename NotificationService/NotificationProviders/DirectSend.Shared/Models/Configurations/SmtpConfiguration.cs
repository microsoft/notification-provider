// Copyright(c) Microsoft Corporation.
// Licensed under the MIT License.

namespace DirectSend.Models.Configurations
{
    /// <summary>
    /// SmtpConfiguration.
    /// </summary>
    public class SmtpConfiguration : ISmtpConfiguration
    {
        /// <summary>
        /// Gets or sets the SMTP server.
        /// </summary>
        /// <value>
        /// The SMTP server.
        /// </value>
        public string SmtpServer { get; set; }

        /// <summary>
        /// Gets or sets the SMTP port.
        /// </summary>
        /// <value>
        /// The SMTP port.
        /// </value>
        public int SmtpPort { get; set; }

        /// <summary>
        /// Gets or sets the SMTP username.
        /// </summary>
        /// <value>
        /// The SMTP username.
        /// </value>
        public string SmtpUsername { get; set; }

        /// <summary>
        /// Gets or sets the SMTP password.
        /// </summary>
        /// <value>
        /// The SMTP password.
        /// </value>
        public string SmtpPassword { get; set; }
    }
}
