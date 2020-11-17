// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace DirectSend.Models.Mail
{
    using System.Collections.Generic;

    public class Recipients
    {
        public IEnumerable<EmailAddress> EmailAddresses { get; set; }

        public RecipientsType RecipientsType { get; set; }
    }
}
