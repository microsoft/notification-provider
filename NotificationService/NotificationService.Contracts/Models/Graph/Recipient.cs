// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Contracts
{
    using System.Runtime.Serialization;

    /// <summary>
    /// Recipient of the MS Graph message.
    /// </summary>
    [DataContract]
    public class Recipient
    {
        /// <summary>
        /// Gets or sets email address of the recipient.
        /// </summary>
        [DataMember(Name = "emailAddress", IsRequired = true)]
        public EmailAddress EmailAddress { get; set; }
    }
}
