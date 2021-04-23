// Copyright(c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Contracts.Extensions
{
    using System;
    using NotificationService.Contracts.Entities;

    /// <summary>
    /// MeetingNotificationItemCosmosDbEntityExtension.
    /// </summary>
    public static class MeetingNotificationItemCosmosDbEntityExtension
    {
        /// <summary>
        /// Converts <see cref="MeetingNotificationItemCosmosDbEntity"/> to a <see cref="MeetingNotificationItemEntity"/>.
        /// </summary>
        /// <param name="meetingNotificationItemDbEntity"> MeetingNotificationItemCosmosDbEntity. </param>
        /// <returns><see cref="MeetingNotificationItemEntity"/>.</returns>
        public static MeetingNotificationItemEntity ConvertToMeetingNotificationItemEntity(this MeetingNotificationItemCosmosDbEntity meetingNotificationItemDbEntity)
        {
            if (meetingNotificationItemDbEntity is null)
            {
                return null;
            }

            MeetingNotificationItemEntity meetingNotificationItemEntity = new MeetingNotificationItemEntity();
            meetingNotificationItemEntity.Application = meetingNotificationItemDbEntity.Application;
            meetingNotificationItemEntity.NotificationId = meetingNotificationItemDbEntity.NotificationId;
            meetingNotificationItemEntity.RequiredAttendees = meetingNotificationItemDbEntity.RequiredAttendees;
            meetingNotificationItemEntity.OptionalAttendees = meetingNotificationItemDbEntity.OptionalAttendees;
            meetingNotificationItemEntity.Application = meetingNotificationItemDbEntity.Application;
            meetingNotificationItemEntity.ErrorMessage = meetingNotificationItemDbEntity.ErrorMessage;
            meetingNotificationItemEntity.DayofMonth = meetingNotificationItemDbEntity.DayofMonth;
            meetingNotificationItemEntity.DayOfWeekByMonth = meetingNotificationItemDbEntity.DayOfWeekByMonth;
            meetingNotificationItemEntity.DaysOfWeek = meetingNotificationItemDbEntity.DaysOfWeek;
            meetingNotificationItemEntity.IsAllDayEvent = meetingNotificationItemDbEntity.IsAllDayEvent;
            meetingNotificationItemEntity.From = meetingNotificationItemDbEntity.From;
            meetingNotificationItemEntity.IsCancel = meetingNotificationItemDbEntity.IsCancel;
            meetingNotificationItemEntity.IsOnlineMeeting = meetingNotificationItemDbEntity.IsOnlineMeeting;
            meetingNotificationItemEntity.NotificationId = meetingNotificationItemDbEntity.NotificationId;
            meetingNotificationItemEntity.Priority = meetingNotificationItemDbEntity.Priority == null ? NotificationPriority.Low : (NotificationPriority)Enum.Parse(typeof(NotificationPriority), meetingNotificationItemDbEntity.Priority);
            meetingNotificationItemEntity.IsPrivate = meetingNotificationItemDbEntity.IsPrivate;
            meetingNotificationItemEntity.IsResponseRequested = meetingNotificationItemDbEntity.IsResponseRequested;
            meetingNotificationItemEntity.Status = meetingNotificationItemDbEntity.Status == null ? NotificationItemStatus.Queued : (NotificationItemStatus)Enum.Parse(typeof(NotificationItemStatus), meetingNotificationItemDbEntity.Status);
            meetingNotificationItemEntity.Subject = meetingNotificationItemDbEntity.Subject;
            meetingNotificationItemEntity.Location = meetingNotificationItemDbEntity.Location;
            meetingNotificationItemEntity.Timestamp = meetingNotificationItemDbEntity.Timestamp;
            meetingNotificationItemEntity.MonthOfYear = meetingNotificationItemDbEntity.MonthOfYear;
            meetingNotificationItemEntity.TrackingId = meetingNotificationItemDbEntity.TrackingId;
            meetingNotificationItemEntity.TryCount = meetingNotificationItemDbEntity.TryCount;
            meetingNotificationItemEntity.ETag = meetingNotificationItemDbEntity.ETag;
            meetingNotificationItemEntity.OccurrenceId = meetingNotificationItemDbEntity.OccurrenceId;
            meetingNotificationItemEntity.Ocurrences = meetingNotificationItemDbEntity.Ocurrences;
            meetingNotificationItemEntity.RecurrencePattern = meetingNotificationItemDbEntity.RecurrencePattern == null ? MeetingRecurrencePattern.None : (MeetingRecurrencePattern)Enum.Parse(typeof(MeetingRecurrencePattern), meetingNotificationItemDbEntity.RecurrencePattern);
            meetingNotificationItemEntity.ReminderMinutesBeforeStart = meetingNotificationItemDbEntity.ReminderMinutesBeforeStart;
            meetingNotificationItemEntity.TemplateId = meetingNotificationItemDbEntity.TemplateId;
            meetingNotificationItemEntity.End = meetingNotificationItemDbEntity.End;
            meetingNotificationItemEntity.Start = meetingNotificationItemDbEntity.Start;
            meetingNotificationItemEntity.EndDate = meetingNotificationItemDbEntity.EndDate;
            meetingNotificationItemEntity.SequenceNumber = meetingNotificationItemDbEntity.SequenceNumber;
            meetingNotificationItemEntity.SendOnUtcDate = meetingNotificationItemDbEntity.SendOnUtcDate;
            meetingNotificationItemEntity.TrackingId = meetingNotificationItemDbEntity.TrackingId;
            meetingNotificationItemEntity.TryCount = meetingNotificationItemDbEntity.TryCount;
            meetingNotificationItemEntity.Timestamp = meetingNotificationItemDbEntity.Timestamp;
            meetingNotificationItemEntity.RowKey = meetingNotificationItemDbEntity.NotificationId;
            meetingNotificationItemEntity.PartitionKey = meetingNotificationItemDbEntity.Application;
            meetingNotificationItemEntity.Interval = meetingNotificationItemDbEntity.Interval;
            meetingNotificationItemEntity.ICalUid = meetingNotificationItemDbEntity.ICalUid;
            meetingNotificationItemEntity.AttachmentReference = meetingNotificationItemDbEntity.AttachmentReference;
            meetingNotificationItemEntity.EventId = meetingNotificationItemDbEntity.EventId;
            meetingNotificationItemEntity.EmailAccountUsed = meetingNotificationItemDbEntity.EmailAccountUsed;
            meetingNotificationItemEntity.Action = meetingNotificationItemDbEntity.Action;
            meetingNotificationItemEntity.Id = meetingNotificationItemDbEntity.Id;
            return meetingNotificationItemEntity;
        }
    }
}
