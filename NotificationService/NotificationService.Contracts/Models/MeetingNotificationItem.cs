// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Contracts.Models
{

    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Runtime.Serialization;
    using NotificationService.Common.CustomValidations;

    /// <summary>
    /// MeetingNotificationItem.
    /// </summary>
    /// <seealso cref="NotificationService.Contracts.NotificationItemBase" />
    [DataContract]
    public class MeetingNotificationItem : NotificationItemBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MeetingNotificationItem"/> class.
        /// </summary>
        public MeetingNotificationItem()
        {
            this.Priority = NotificationPriority.Normal;
            this.Attachments = Array.Empty<NotificationAttachment>();
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
        /// Gets or Sets Action.
        /// </summary>
        [DataMember(Name = "action")]
        public string Action { get; set; }

        /// <summary>
        /// Gets or sets the priority.
        /// </summary>
        [DataMember(Name = "priority")]
        public NotificationPriority Priority { get; set; }

        /// <summary>
        /// Gets or sets from.
        /// </summary>
        [DataMember(Name = "from")]
        [Required(ErrorMessage = "'From' is mandatory for meeting notifications.")]
        public string From { get; set; }

        /// <summary>
        /// Gets or sets to.
        /// </summary>
        [DataMember(Name = "requiredAttendees")]
        [Required(ErrorMessage = "'RequiredAttendees' is mandatory for meeting notifications.")]
        [EmailIdListValidation(PropertyName = "RequiredAttendees", Nullable = false)]
        public string RequiredAttendees { get; set; }

        /// <summary>
        /// Gets or sets the cc.
        /// </summary>
        [DataMember(Name = "optionalAttendees")]
        [EmailIdListValidation(PropertyName = "OptionalAttendees", Nullable = true)]
        public string OptionalAttendees { get; set; }

        /// <summary>
        /// Gets or sets the subject.
        /// </summary>
        [DataMember(Name = "subject")]
        [Required(ErrorMessage = "'Subject' is mandatory for meeting notifications.")]
        public string Subject { get; set; }

        /// <summary>
        /// Gets or sets the body.
        /// </summary>
        [DataMember(Name = "body")]
        public string Body { get; set; }

        /// <summary>
        /// Gets or sets the attachments.
        /// </summary>
        [DataMember(Name = "attachments")]
        public IEnumerable<NotificationAttachment> Attachments { get; set; }

        /// <summary>
        /// Gets or sets the ReminderMinutesBeforeStart.
        /// </summary>
        [DataMember(Name = "reminderMinutesBeforeStart")]
        public string ReminderMinutesBeforeStart { get; set; }

        /// <summary>
        /// Gets or sets the Location.
        /// </summary>
        [DataMember(Name = "location")]
        public string Location { get; set; }

        /// <summary>
        /// Gets or sets the Start.
        /// </summary>
        [DataMember(Name = "Start")]
        [Required(ErrorMessage = "'Start' is mandatory for meeting notifications.")]
        public DateTime Start { get; set; }

        /// <summary>
        /// Gets or sets the End.
        /// </summary>
        [DataMember(Name = "End")]
        [Required(ErrorMessage = "'End' is mandatory for meeting notifications.")]
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
        public MeetingRecurrencePattern RecurrencePattern { get; set; }

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
        /// Gets or sets a value indicating whether gets or sets the IsOnlineMeeting.Not Available for Direct Send.
        /// </summary>
        [DataMember(Name = "IsOnlineMeeting")]
        public bool IsOnlineMeeting { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether gets or sets the IsResponseRequested.
        /// </summary>
        [DataMember(Name = "IsResponseRequested")]
        public bool IsResponseRequested { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether gets or sets the IsCancel.Currently Cancel Meeting is not implemented.
        /// </summary>
        [DataMember(Name = "IsCancel")]
        public bool IsCancel { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether gets or sets the IsPrivate.
        /// </summary>
        [DataMember(Name = "IsPrivate")]
        public bool IsPrivate { get; set; }

        /// <summary>
        /// Gets or sets the TemplateId.
        /// </summary>
        [DataMember(Name = "TemplateId")]
        public string TemplateId { get; set; }

        /// <summary>
        /// Gets or sets the Template Content Arguments.
        /// </summary>
        [DataMember(Name = "TemplateData")]
        public string TemplateData { get; set; }

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
    }
}
