// Copyright(c) Microsoft Corporation.
// Licensed under the MIT License.

namespace DirectSend.Models.Configurations
{
    /// <summary>
    /// ISmtpConfiguration.
    /// </summary>
    public interface ISmtpConfiguration
    {
        /// <summary>
        /// Gets the SMTP server.
        /// </summary>
        /// <value>
        /// The SMTP server.
        /// </value>
        string SmtpServer { get; }

        /// <summary>
        /// Gets the SMTP port.
        /// </summary>
        /// <value>
        /// The SMTP port.
        /// </value>
        int SmtpPort { get; }

        /// <summary>
        /// Gets or sets the SMTP username.
        /// </summary>
        /// <value>
        /// The SMTP username.
        /// </value>
        string SmtpUsername { get; set; }

        /// <summary>
        /// Gets or sets the SMTP password.
        /// </summary>
        /// <value>
        /// The SMTP password.
        /// </value>
        string SmtpPassword { get; set; }
    }
}
