// Copyright(c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Contracts.Extensions
{
    using System;
    using NotificationService.Contracts.Entities;

    /// <summary>
    /// MeetingNotificationItemTableEntityExtension.
    /// </summary>
    public static class MeetingNotificationItemTableEntityExtension
    {
        /// <summary>
        /// Converts <see cref="MeetingNotificationItemTableEntity"/> to a <see cref="MeetingNotificationItemEntity"/>.
        /// </summary>
        /// <param name="meetingNotificationItemTableEntity"> MeetingNotificationItemTableEntitys. </param>
        /// <returns><see cref="MeetingNotificationItemEntity"/>.</returns>
        public static MeetingNotificationItemEntity ConvertToMeetingNotificationItemEntity(this MeetingNotificationItemTableEntity meetingNotificationItemTableEntity)
        {
            if (meetingNotificationItemTableEntity is null)
            {
                return null;
            }

            MeetingNotificationItemEntity meetingNotificationItemEntity = new MeetingNotificationItemEntity();
            meetingNotificationItemEntity.Application = meetingNotificationItemTableEntity.Application;
            meetingNotificationItemEntity.NotificationId = meetingNotificationItemTableEntity.NotificationId;
            meetingNotificationItemEntity.RequiredAttendees = meetingNotificationItemTableEntity.RequiredAttendees;
            meetingNotificationItemEntity.OptionalAttendees = meetingNotificationItemTableEntity.OptionalAttendees;
            meetingNotificationItemEntity.Application = meetingNotificationItemTableEntity.Application;
            meetingNotificationItemEntity.ErrorMessage = meetingNotificationItemTableEntity.ErrorMessage;
            meetingNotificationItemEntity.DayofMonth = meetingNotificationItemTableEntity.DayofMonth;
            meetingNotificationItemEntity.DayOfWeekByMonth = meetingNotificationItemTableEntity.DayOfWeekByMonth;
            meetingNotificationItemEntity.DaysOfWeek = meetingNotificationItemTableEntity.DaysOfWeek;
            meetingNotificationItemEntity.IsAllDayEvent = meetingNotificationItemTableEntity.IsAllDayEvent;
            meetingNotificationItemEntity.From = meetingNotificationItemTableEntity.From;
            meetingNotificationItemEntity.IsCancel = meetingNotificationItemTableEntity.IsCancel;
            meetingNotificationItemEntity.IsOnlineMeeting = meetingNotificationItemTableEntity.IsOnlineMeeting;
            meetingNotificationItemEntity.NotificationId = meetingNotificationItemTableEntity.NotificationId;
            meetingNotificationItemEntity.Priority = meetingNotificationItemTableEntity.Priority == null ? NotificationPriority.Low : (NotificationPriority)Enum.Parse(typeof(NotificationPriority), meetingNotificationItemTableEntity.Priority);
            meetingNotificationItemEntity.IsPrivate = meetingNotificationItemTableEntity.IsPrivate;
            meetingNotificationItemEntity.IsResponseRequested = meetingNotificationItemTableEntity.IsResponseRequested;
            meetingNotificationItemEntity.Status = meetingNotificationItemTableEntity.Status == null ? NotificationItemStatus.Queued : (NotificationItemStatus)Enum.Parse(typeof(NotificationItemStatus), meetingNotificationItemTableEntity.Status);
            meetingNotificationItemEntity.Subject = meetingNotificationItemTableEntity.Subject;
            meetingNotificationItemEntity.Location = meetingNotificationItemTableEntity.Location;
            meetingNotificationItemEntity.Timestamp = meetingNotificationItemTableEntity.Timestamp;
            meetingNotificationItemEntity.MonthOfYear = meetingNotificationItemTableEntity.MonthOfYear;
            meetingNotificationItemEntity.TrackingId = meetingNotificationItemTableEntity.TrackingId;
            meetingNotificationItemEntity.TryCount = meetingNotificationItemTableEntity.TryCount;
            meetingNotificationItemEntity.ETag = meetingNotificationItemTableEntity.ETag;
            meetingNotificationItemEntity.OccurrenceId = meetingNotificationItemTableEntity.OccurrenceId;
            meetingNotificationItemEntity.Ocurrences = meetingNotificationItemTableEntity.Ocurrences;
            meetingNotificationItemEntity.RecurrencePattern = meetingNotificationItemTableEntity.RecurrencePattern == null ? MeetingRecurrencePattern.None : (MeetingRecurrencePattern)Enum.Parse(typeof(MeetingRecurrencePattern), meetingNotificationItemTableEntity.RecurrencePattern);
            meetingNotificationItemEntity.ReminderMinutesBeforeStart = meetingNotificationItemTableEntity.ReminderMinutesBeforeStart;
            meetingNotificationItemEntity.TemplateId = meetingNotificationItemTableEntity.TemplateId;
            meetingNotificationItemEntity.End = meetingNotificationItemTableEntity.End;
            meetingNotificationItemEntity.Start = meetingNotificationItemTableEntity.Start;
            meetingNotificationItemEntity.EndDate = meetingNotificationItemTableEntity.EndDate;
            meetingNotificationItemEntity.SequenceNumber = meetingNotificationItemTableEntity.SequenceNumber;
            meetingNotificationItemEntity.SendOnUtcDate = meetingNotificationItemTableEntity.SendOnUtcDate;
            meetingNotificationItemEntity.TrackingId = meetingNotificationItemTableEntity.TrackingId;
            meetingNotificationItemEntity.TryCount = meetingNotificationItemTableEntity.TryCount;
            meetingNotificationItemEntity.Timestamp = meetingNotificationItemTableEntity.Timestamp;
            meetingNotificationItemEntity.RowKey = meetingNotificationItemTableEntity.NotificationId;
            meetingNotificationItemEntity.PartitionKey = meetingNotificationItemTableEntity.Application;
            meetingNotificationItemEntity.Interval = meetingNotificationItemTableEntity.Interval;
            meetingNotificationItemEntity.ICalUid = meetingNotificationItemTableEntity.ICalUid;
            meetingNotificationItemEntity.AttachmentReference = meetingNotificationItemTableEntity.AttachmentReference;
            meetingNotificationItemEntity.EventId = meetingNotificationItemTableEntity.EventId;
            meetingNotificationItemEntity.EmailAccountUsed = meetingNotificationItemTableEntity.EmailAccountUsed;
            meetingNotificationItemEntity.Action = meetingNotificationItemTableEntity.Action;
            return meetingNotificationItemEntity;
        }
    }
}
