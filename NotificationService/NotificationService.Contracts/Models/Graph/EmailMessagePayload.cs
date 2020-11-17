// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Contracts
{
    using System.Runtime.Serialization;

    /// <summary>
    /// Payload to be sent to Graph API to send an email message.
    /// </summary>
    [DataContract]
    public class EmailMessagePayload
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EmailMessagePayload"/> class.
        /// </summary>
        /// <param name="emailMessage">Email message to be sent to graph.</param>
        public EmailMessagePayload(EmailMessage emailMessage)
        {
            this.Message = emailMessage;
        }

        /// <summary>
        /// Gets or sets the message to be sent to Graph.
        /// </summary>
        [DataMember(Name = "message", IsRequired = true)]
        public EmailMessage Message { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the email message is saved in Sent items folder.
        /// </summary>
        [DataMember(Name = "saveToSentItems", IsRequired = false)]
        public bool SaveToSentItems { get; set; }
    }
}
