// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Contracts.Entities
{
    using System;
    using System.Runtime.Serialization;
    using Microsoft.Azure.Cosmos.Table;

    /// <summary>
    /// MeetingNotificationItemEntity.
    /// </summary>
    public class MeetingNotificationItemTableEntity : TableEntity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MeetingNotificationItemTableEntity"/> class.
        /// </summary>
        public MeetingNotificationItemTableEntity()
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
        public string From { get; set; }

        /// <summary>
        /// Gets or sets to.
        /// </summary>
        public string RequiredAttendees { get; set; }

        /// <summary>
        /// Gets or sets the cc.
        /// </summary>
        public string OptionalAttendees { get; set; }

        /// <summary>
        /// Gets or sets the subject.
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// Gets or sets the ReminderMinutesBeforeStart.
        /// </summary>
        public string ReminderMinutesBeforeStart { get; set; }

        /// <summary>
        /// Gets or sets the Location.
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        /// Gets or sets the Start.
        /// </summary>
        public DateTime Start { get; set; }

        /// <summary>
        /// Gets or sets the End.
        /// </summary>
        public DateTime End { get; set; }

        /// <summary>
        /// Gets or sets the End date.
        /// </summary>
        public DateTime? EndDate { get; set; }

        // Recurrence Properties

        /// <summary>
        /// Gets or sets the Recurrence pattern.
        /// </summary>
        public string RecurrencePattern { get; set; }

        /// <summary>
        /// Gets or sets the ICalUid.
        /// </summary>
        public string ICalUid { get; set; }

        /// <summary>
        /// Gets or sets the Interval.
        /// </summary>
        public int Interval { get; set; }

        /// <summary>
        /// Gets or sets the DaysOfWeek.
        /// </summary>
        public string DaysOfWeek { get; set; }

        /// <summary>
        /// Gets or sets the DayofMonth.
        /// </summary>
        public int DayofMonth { get; set; }

        /// <summary>
        /// Gets or sets the Day of the week index for a Monthly recurring pattern.
        /// </summary>
        public string DayOfWeekByMonth { get; set; }

        /// <summary>
        /// Gets or sets the MonthOfYear.
        /// </summary>
        public int MonthOfYear { get; set; }

        /// <summary>
        /// Gets or sets the ocurrence.
        /// </summary>
        public int Ocurrences { get; set; }

        // other properties

        /// <summary>
        /// Gets or sets a value indicating whether gets or sets the IsAllDayEvent.
        /// </summary>
        public bool IsAllDayEvent { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether gets or sets the IsOnlineMeeting.
        /// </summary>
        public bool IsOnlineMeeting { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether gets or sets the IsResponseRequested.
        /// </summary>
        public bool IsResponseRequested { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether gets or sets the IsCancel.
        /// </summary>
        public bool IsCancel { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether gets or sets the IsPrivate.
        /// </summary>
        public bool IsPrivate { get; set; }

        /// <summary>
        /// Gets or sets the TemplateId.
        /// </summary>
        public string TemplateId { get; set; }

        /// <summary>
        /// Gets or sets the OccurrenceId.
        /// </summary>
        public DateTime? OccurrenceId { get; set; }

        /// <summary>
        /// Gets or sets the SequenceNumber. Applicable for SMTP only.
        /// </summary>
        public int SequenceNumber { get; set; }

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
        ///  Gets or sets attachment Reference to blob.
        /// </summary>
        public string AttachmentReference { get; set; }

        /// <summary>
        /// Gets or sets Mailbox Account used to deliver the email.
        /// </summary>
        public string EmailAccountUsed { get; set; }

        /// <summary>
        /// Gets or Sets EventId. An unique Id from Graph API to send attachments to the same event.
        /// </summary>
        public string EventId { get; set; }

        /// <summary>
        /// Gets or Sets Action.
        /// </summary>
        public string Action { get; set; }
    }
}
