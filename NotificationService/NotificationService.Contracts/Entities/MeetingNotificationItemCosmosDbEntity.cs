// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Contracts.Entities
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Microsoft.Azure.Cosmos.Table;

    /// <summary>
    /// MeetingNotificationItemCosmosDbEntity.
    /// </summary>
    public class MeetingNotificationItemCosmosDbEntity : CosmosDBEntity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MeetingNotificationItemCosmosDbEntity"/> class.
        /// </summary>
        public MeetingNotificationItemCosmosDbEntity()
        {
            this.Priority = NotificationPriority.Normal.ToString();
            this.SendOnUtcDate = DateTime.UtcNow;
            this.TrackingId = string.Empty;
        }

        /// <summary>
        /// Gets the type of the notify.
        /// </summary>
        [DataMember(Name = "notifyType")]
        public static string NotifyType => NotificationType.Meet.ToString();

        /// <summary>
        /// Gets or sets Application associated to the notification item.
        /// </summary>
        [DataMember(Name = "Application")]
        public string Application { get; set; }

        /// <summary>
        /// Gets or sets the priority.
        /// </summary>
        [DataMember(Name = "priority")]
        public string Priority { get; set; }

        /// <summary>
        /// Gets or sets from.
        /// </summary>
        [DataMember(Name = "From")]
        public string From { get; set; }

        /// <summary>
        /// Gets or sets to.
        /// </summary>
        [DataMember(Name = "RequiredAttendees")]
        public string RequiredAttendees { get; set; }

        /// <summary>
        /// Gets or sets the cc.
        /// </summary>
        [DataMember(Name = "OptionalAttendees")]
        public string OptionalAttendees { get; set; }

        /// <summary>
        /// Gets or sets the subject.
        /// </summary>
        [DataMember(Name = "Subject")]
        public string Subject { get; set; }

        /// <summary>
        /// Gets or sets the ReminderMinutesBeforeStart.
        /// </summary>
        [DataMember(Name = "ReminderMinutesBeforeStart")]
        public string ReminderMinutesBeforeStart { get; set; }

        /// <summary>
        /// Gets or sets the Location.
        /// </summary>
        [DataMember(Name = "Location")]
        public string Location { get; set; }

        /// <summary>
        /// Gets or sets the Start.
        /// </summary>
        [DataMember(Name = "Start")]
        public DateTime Start { get; set; }

        /// <summary>
        /// Gets or sets the End.
        /// </summary>
        [DataMember(Name = "End")]
        public DateTime End { get; set; }

        /// <summary>
        /// Gets or sets the End date.
        /// </summary>
        [DataMember(Name = "EndDate")]
        public DateTime? EndDate { get; set; }

        // Recurrence Properties

        /// <summary>
        /// Gets or sets the Recurrence pattern.
        /// </summary>
        [DataMember(Name = "RecurrencePattern")]
        public string RecurrencePattern { get; set; }

        /// <summary>
        /// Gets or sets the ICalUid.
        /// </summary>
        [DataMember(Name = "ICalUid")]
        public string ICalUid { get; set; }

        /// <summary>
        /// Gets or sets the Interval.
        /// </summary>
        [DataMember(Name = "Interval")]
        public int Interval { get; set; }

        /// <summary>
        /// Gets or sets the DaysOfWeek.
        /// </summary>
        [DataMember(Name = "DaysOfWeek")]
        public string DaysOfWeek { get; set; }

        /// <summary>
        /// Gets or sets the DayofMonth.
        /// </summary>
        [DataMember(Name = "DayofMonth")]
        public int? DayofMonth { get; set; }

        /// <summary>
        /// Gets or sets the Day of the week index for a Monthly recurring pattern.
        /// </summary>
        [DataMember(Name = "DayOfWeekByMonth")]
        public string DayOfWeekByMonth { get; set; }

        /// <summary>
        /// Gets or sets the MonthOfYear.
        /// </summary>
        [DataMember(Name = "MonthOfYear")]
        public int MonthOfYear { get; set; }

        /// <summary>
        /// Gets or sets the ocurrence.
        /// </summary>
        [DataMember(Name = "Ocurrences")]
        public int? Ocurrences { get; set; }

        // other properties

        /// <summary>
        /// Gets or sets a value indicating whether gets or sets the IsAllDayEvent.
        /// </summary>
        [DataMember(Name = "IsAllDayEvent")]
        public bool IsAllDayEvent { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether gets or sets the IsOnlineMeeting.
        /// </summary>
        [DataMember(Name = "IsOnlineMeeting")]
        public bool IsOnlineMeeting { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether gets or sets the IsResponseRequested.
        /// </summary>
        [DataMember(Name = "IsResponseRequested")]
        public bool IsResponseRequested { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether gets or sets the IsCancel.
        /// </summary>
        [DataMember(Name = "IsCancel")]
        public bool IsCancel { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether gets or sets the IsPrivate.
        /// </summary>
        [DataMember(Name = "IsPrivate")]
        public bool IsPrivate { get; set; }

        /// <summary>
        /// Gets or sets the Template Content Arguments.
        /// </summary>
        [DataMember(Name = "TemplateId")]
        public string TemplateId { get; set; }

        /// <summary>
        /// Gets or sets the OccurrenceId.
        /// </summary>
        [DataMember(Name = "OccurrenceId")]
        public DateTime? OccurrenceId { get; set; }

        /// <summary>
        /// Gets or sets the SequenceNumber. Applicable for SMTP only.
        /// </summary>
        [DataMember(Name = "SequenceNumber")]
        public int? SequenceNumber { get; set; }

        /// <summary>
        /// Gets or sets Unique Identifier for the Notification Item.
        /// </summary>
        [DataMember(Name = "NotificationId")]
        public string NotificationId { get; set; }

        /// <summary>
        /// Gets or sets the send on UTC date.
        /// </summary>
        [DataMember(Name = "SendOnUtcDate")]
        public DateTime SendOnUtcDate { get; set; }

        /// <summary>
        /// Gets or sets the TrackingId.
        /// </summary>
        [DataMember(Name = "TrackingId")]
        public string TrackingId { get; set; }

        /// <summary>
        /// Gets or sets status of the Notification item.
        /// </summary>
        [DataMember(Name = "Status")]
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets a counter value on how many attempts made to send the notification item.
        /// </summary>
        [DataMember(Name = "TryCount")]
        public int TryCount { get; set; }

        /// <summary>
        /// Gets or sets error message when the email processing failed.
        /// </summary>
        [DataMember(Name = "ErrorMessage")]
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the attachment reference.
        /// </summary>
        [DataMember(Name = "AttachmentReference")]
        public string AttachmentReference { get; set; }

        /// <summary>
        /// Gets or sets Mailbox Account used to deliver the email.
        /// </summary>
        [DataMember(Name = "EmailAccountUsed")]
        public string EmailAccountUsed { get; set; }

        /// <summary>
        /// Gets or Sets EventId. An unique Id from Graph API to send attachments to the same event. 
        /// </summary>
        [DataMember(Name = "EventId")]
        public string EventId { get; set; }

        /// <summary>
        /// Gets or Sets Action (Create, Update, Delete).
        /// </summary>
        [DataMember(Name = "Action")]
        public string Action { get; set; }
    }
}
