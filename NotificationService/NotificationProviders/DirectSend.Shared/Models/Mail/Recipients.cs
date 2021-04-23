// Copyright(c) Microsoft Corporation.
// Licensed under the MIT License.

namespace DirectSend.Models.Mail
{
    using System.Collections.Generic;

    /// <summary>
    /// Recipients Model.
    /// </summary>
    public class Recipients
    {
        /// <summary>
        /// Gets or sets EmailAddresses.
        /// </summary>
        public IEnumerable<EmailAddress> EmailAddresses { get; set; }

        /// <summary>
        /// Gets or sets RecipientsType.
        /// </summary>
        public RecipientsType RecipientsType { get; set; }
    }
}
