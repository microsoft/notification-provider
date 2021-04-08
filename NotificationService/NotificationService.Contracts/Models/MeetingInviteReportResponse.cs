// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Contracts
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Notification Report Response.
    /// </summary>
    [DataContract]
    public class MeetingInviteReportResponse
    {
        /// <summary>
        /// Gets or sets Notification Id.
        /// </summary>
        [DataMember(Name = "NotificationId")]
        public string NotificationId { get; set; }

        /// <summary>
        /// Gets or sets Application.
        /// </summary>
        [DataMember(Name = "Application")]
        public string Application { get; set; }

        /// <summary>
        /// Gets or sets EmailAccountUsed.
        /// </summary>
        [DataMember(Name = "EmailAccountUsed")]
        public string EmailAccountUsed { get; set; }

        /// <summary>
        /// Gets or sets Tracking Id.
        /// </summary>
        [DataMember(Name = "TrackingId")]
        public string TrackingId { get; set; }


        /// <summary>
        /// Gets or sets Status.
        /// </summary>
        [DataMember(Name = "Status")]
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets Priority.
        /// </summary>
        [DataMember(Name = "Priority")]
        public string Priority { get; set; }

        /// <summary>
        /// Gets or sets Priority.
        /// </summary>
        [DataMember(Name = "TryCount")]
        public int TryCount { get; set; }

        /// <summary>
        /// Gets or sets ErrorMessage, if notification failed.
        /// </summary>
        [DataMember(Name = "ErrorMessage")]
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets CreatedDateTime.
        /// </summary>
        [DataMember(Name = "CreatedDateTime")]
        public DateTime CreatedDateTime { get; set; }

        /// <summary>
        /// Gets or sets UpdatedDateTime.
        /// </summary>
        [DataMember(Name = "UpdatedDateTime")]
        public DateTime UpdatedDateTime { get; set; }

        /// <summary>
        /// Gets or sets SendOnUtcDate.
        /// </summary>
        [DataMember(Name = "SendOnUtcDate")]
        public DateTime SendOnUtcDate { get; set; }

        /// <summary>
        /// Gets or sets RequiredAttendees.
        /// </summary>
        [DataMember(Name = "RequiredAttendees")]
        public string RequiredAttendees { get; set; }

        /// <summary>
        /// Gets or sets From.
        /// </summary>
        [DataMember(Name = "From")]
        public string From { get; set; }

        /// <summary>
        /// Gets or sets OptionalAttendees.
        /// </summary>
        [DataMember(Name = "OptionalAttendees")]
        public string OptionalAttendees { get; set; }

        /// <summary>
        /// Gets or sets Subject.
        /// </summary>
        [DataMember(Name = "Subject")]
        public string Subject { get; set; }

    }
}
