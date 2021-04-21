// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Contracts.Models.Reports
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    /// <summary>
    /// MeetingNotification message for reporting.
    /// </summary>
    public class MeetingInviteMessage
    {
        /// <summary>
        /// Gets or sets the priority.
        /// </summary>
        [DataMember(Name = "Application")]
        public string Application { get; set; }

        /// <summary>
        /// Gets or sets from.
        /// </summary>
        [DataMember(Name = "from")]
        public string From { get; set; }

        /// <summary>
        /// Gets or sets to.
        /// </summary>
        [DataMember(Name = "requiredAttendees")]
        public string RequiredAttendees { get; set; }

        /// <summary>
        /// Gets or sets the cc.
        /// </summary>
        [DataMember(Name = "optionalAttendees")]
        public string OptionalAttendees { get; set; }

        /// <summary>
        /// Gets or sets the subject.
        /// </summary>
        [DataMember(Name = "subject")]
        public string Subject { get; set; }

        /// <summary>
        /// Gets or sets the body.
        /// </summary>
        [DataMember(Name = "body")]
        public string Body { get; set; }

        /// <summary>
        /// Gets or Sets NotificationId.
        /// </summary>
        [DataMember(Name = "NotificationId")]
        public string NotificationId { get; set; }

        /// <summary>
        /// Gets or sets attachments to the message.
        /// </summary>
        [DataMember(Name = "attachments", IsRequired = false)]
        public List<FileAttachment> Attachments { get; set; }
    }
}
