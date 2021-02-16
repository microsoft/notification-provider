// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Contracts.Entities
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    /// <summary>
    /// MeetingNotificationItemEntity.
    /// </summary>
    /// <seealso cref="NotificationService.Contracts.NotificationItemBaseEntity" />
    public class MeetingNotificationItemEntity : NotificationItemBaseEntity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MeetingNotificationItemEntity"/> class.
        /// </summary>
        public MeetingNotificationItemEntity()
        {
            this.Priority = NotificationPriority.Normal;
            this.Attachments = Array.Empty<NotificationAttachmentEntity>();
            this.SendOnUtcDate = DateTime.UtcNow;
            this.TrackingId = string.Empty;
        }

        /// <summary>
        /// Gets the type of the notify.
        /// </summary>
        [DataMember(Name = "notifyType")]
        public override NotificationType NotifyType
        {
            get { return NotificationType.Meet; }
        }

        /// <summary>
        /// Gets or sets Application associated to the notification item.
        /// </summary>
        [DataMember(Name = "Application")]
        public string Application { get; set; }

        /// <summary>
        /// Gets or sets the priority.
        /// </summary>
        [DataMember(Name = "priority")]
        public NotificationPriority Priority { get; set; }

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
        /// Gets or sets the body.
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// Gets or sets the attachments.
        /// </summary>
        public IEnumerable<NotificationAttachmentEntity> Attachments { get; set; }

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
        public MeetingRecurrencePattern RecurrencePattern { get; set; }

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
        public int? DayofMonth { get; set; }

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
        public int? Ocurrences { get; set; }

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
        /// Gets or sets the Template Content Arguments.
        /// </summary>
        public string TemplateData { get; set; }

        /// <summary>
        /// Gets or sets the Template Content Arguments.
        /// </summary>
        public string TemplateId { get; set; }

        /// <summary>
        /// Gets or sets the OccurrenceId.
        /// </summary>
        public DateTime? OccurrenceId { get; set; }

        /// <summary>
        /// Gets or sets the SequenceNumber. Applicable for SMTP only.
        /// </summary>
        public int? SequenceNumber { get; set; }

        /// <summary>
        /// Gets or sets the attachment reference.
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
        /// Gets or Sets Action (Create, Update, Delete).
        /// </summary>
        public string Action { get; set; }
    }
}
