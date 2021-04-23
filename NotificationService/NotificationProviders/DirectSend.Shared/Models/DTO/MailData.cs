// Copyright(c) Microsoft Corporation.
// Licensed under the MIT License.

namespace DirectSend.Models.DTO
{
    using DirectSend.Models.Mail;

    /// <summary>
    /// MailData.
    /// </summary>
    public class MailData
    {
        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        public EmailMessage Message { get; set; }
    }
}
