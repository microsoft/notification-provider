// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Contracts.Extensions
{
    using System;
    using NotificationService.Contracts.Entities;

    /// <summary>
    /// MeetingNotificationItemEntityExtensions.
    /// </summary>
    public static class MeetingNotificationItemEntityExtensions
    {
        /// <summary>
        /// Converts <see cref="MeetingNotificationItemEntity"/> to a <see cref="MeetingInviteReportResponse"/>.
        /// </summary>
        /// <param name="meetingNotificationItemEntity"> meetingNotificationItemEntity. </param>
        /// <returns><see cref="MeetingInviteReportResponse"/>.</returns>
        public static MeetingInviteReportResponse ToMeetingInviteReportResponse(MeetingNotificationItemEntity meetingNotificationItemEntity)
        {
            return new MeetingInviteReportResponse()
            {
                NotificationId = meetingNotificationItemEntity?.NotificationId,
                Application = meetingNotificationItemEntity?.Application,
                EmailAccountUsed = meetingNotificationItemEntity?.EmailAccountUsed,
                TrackingId = meetingNotificationItemEntity?.TrackingId,
                Status = meetingNotificationItemEntity?.Status.ToString(),
                Priority = meetingNotificationItemEntity?.Priority.ToString(),
                TryCount = meetingNotificationItemEntity?.TryCount ?? 0,
                ErrorMessage = meetingNotificationItemEntity?.ErrorMessage,
                CreatedDateTime = meetingNotificationItemEntity?.CreatedDateTime ?? DateTime.MinValue,
                UpdatedDateTime = meetingNotificationItemEntity?.UpdatedDateTime ?? DateTime.MinValue,
                SendOnUtcDate = meetingNotificationItemEntity?.SendOnUtcDate ?? DateTime.MinValue,
                RequiredAttendees = meetingNotificationItemEntity?.RequiredAttendees,
                From = meetingNotificationItemEntity?.From,
                OptionalAttendees = meetingNotificationItemEntity?.OptionalAttendees,
                Subject = meetingNotificationItemEntity?.Subject,
            };
        }

        /// <summary>
        /// Converts <see cref="MeetingNotificationItemEntity"/> to a <see cref="MeetingNotificationItemTableEntity"/>.
        /// </summary>
        /// <param name="meetingNotificationItemEntity"> meetingNotificationItemEntity. </param>
        /// <returns><see cref="MeetingNotificationItemTableEntity"/>.</returns>
        public static MeetingNotificationItemTableEntity ConvertToMeetingNotificationItemTableEntity(this MeetingNotificationItemEntity meetingNotificationItemEntity)
        {
            if (meetingNotificationItemEntity is null)
            {
                return null;
            }

            MeetingNotificationItemTableEntity meetingNotificationItemTableEntity = new MeetingNotificationItemTableEntity();
            meetingNotificationItemTableEntity.PartitionKey = meetingNotificationItemEntity.Application;
            meetingNotificationItemTableEntity.RowKey = meetingNotificationItemEntity.NotificationId;
            meetingNotificationItemTableEntity.Application = meetingNotificationItemEntity.Application;
            meetingNotificationItemTableEntity.RequiredAttendees = meetingNotificationItemEntity.RequiredAttendees;
            meetingNotificationItemTableEntity.OptionalAttendees = meetingNotificationItemEntity.OptionalAttendees;
            meetingNotificationItemTableEntity.Application = meetingNotificationItemEntity.Application;
            meetingNotificationItemTableEntity.ErrorMessage = meetingNotificationItemEntity.ErrorMessage;
            meetingNotificationItemTableEntity.DayofMonth = meetingNotificationItemEntity.DayofMonth ?? default;
            meetingNotificationItemTableEntity.DayOfWeekByMonth = meetingNotificationItemEntity.DayOfWeekByMonth;
            meetingNotificationItemTableEntity.DaysOfWeek = meetingNotificationItemEntity.DaysOfWeek;
            meetingNotificationItemTableEntity.EndDate = meetingNotificationItemEntity.EndDate;
            meetingNotificationItemTableEntity.IsAllDayEvent = meetingNotificationItemEntity.IsAllDayEvent;
            meetingNotificationItemTableEntity.From = meetingNotificationItemEntity.From;
            meetingNotificationItemTableEntity.IsCancel = meetingNotificationItemEntity.IsCancel;
            meetingNotificationItemTableEntity.IsOnlineMeeting = meetingNotificationItemEntity.IsOnlineMeeting;
            meetingNotificationItemTableEntity.NotificationId = meetingNotificationItemEntity.NotificationId;
            meetingNotificationItemTableEntity.Priority = meetingNotificationItemEntity.Priority.ToString();
            meetingNotificationItemTableEntity.IsPrivate = meetingNotificationItemEntity.IsPrivate;
            meetingNotificationItemTableEntity.IsResponseRequested = meetingNotificationItemEntity.IsResponseRequested;
            meetingNotificationItemTableEntity.Status = meetingNotificationItemEntity.Status.ToString();
            meetingNotificationItemTableEntity.Subject = meetingNotificationItemEntity.Subject;
            meetingNotificationItemTableEntity.Location = meetingNotificationItemEntity.Location;
            meetingNotificationItemTableEntity.Timestamp = meetingNotificationItemEntity.Timestamp;
            meetingNotificationItemTableEntity.MonthOfYear = meetingNotificationItemEntity.MonthOfYear;
            meetingNotificationItemTableEntity.TrackingId = meetingNotificationItemEntity.TrackingId;
            meetingNotificationItemTableEntity.TryCount = meetingNotificationItemEntity.TryCount;
            meetingNotificationItemTableEntity.ETag = meetingNotificationItemEntity.ETag;
            meetingNotificationItemTableEntity.OccurrenceId = meetingNotificationItemEntity.OccurrenceId;
            meetingNotificationItemTableEntity.Ocurrences = meetingNotificationItemEntity.Ocurrences ?? default;
            meetingNotificationItemTableEntity.RecurrencePattern = meetingNotificationItemEntity.RecurrencePattern.ToString();
            meetingNotificationItemTableEntity.ReminderMinutesBeforeStart = meetingNotificationItemEntity.ReminderMinutesBeforeStart;
            meetingNotificationItemTableEntity.TemplateId = meetingNotificationItemEntity.TemplateId;
            meetingNotificationItemTableEntity.End = meetingNotificationItemEntity.End;
            meetingNotificationItemTableEntity.Start = meetingNotificationItemEntity.Start;
            meetingNotificationItemTableEntity.SequenceNumber = meetingNotificationItemEntity.SequenceNumber ?? default;
            meetingNotificationItemTableEntity.SendOnUtcDate = meetingNotificationItemEntity.SendOnUtcDate;
            meetingNotificationItemTableEntity.TrackingId = meetingNotificationItemEntity.TrackingId;
            meetingNotificationItemTableEntity.TryCount = meetingNotificationItemEntity.TryCount;
            meetingNotificationItemTableEntity.Timestamp = meetingNotificationItemEntity.Timestamp;
            meetingNotificationItemTableEntity.RowKey = meetingNotificationItemEntity.NotificationId;
            meetingNotificationItemTableEntity.PartitionKey = meetingNotificationItemEntity.Application;
            meetingNotificationItemTableEntity.Interval = meetingNotificationItemEntity.Interval;
            meetingNotificationItemTableEntity.ICalUid = meetingNotificationItemEntity.ICalUid;
            meetingNotificationItemTableEntity.AttachmentReference = meetingNotificationItemEntity.AttachmentReference;
            meetingNotificationItemTableEntity.EmailAccountUsed = meetingNotificationItemEntity.EmailAccountUsed;
            meetingNotificationItemTableEntity.EventId = meetingNotificationItemEntity.EventId;
            return meetingNotificationItemTableEntity;
        }

        /// <summary>
        /// Converts <see cref="MeetingNotificationItemEntity"/> to a <see cref="MeetingNotificationItemTableEntity"/>.
        /// </summary>
        /// <param name="meetingNotificationItemEntity"> meetingNotificationItemEntity. </param>
        /// <returns><see cref="MeetingNotificationItemTableEntity"/>.</returns>
        public static MeetingNotificationItemCosmosDbEntity ConvertToMeetingNotificationItemCosmosDbEntity(this MeetingNotificationItemEntity meetingNotificationItemEntity)
        {
            if (meetingNotificationItemEntity is null)
            {
                return null;
            }

            MeetingNotificationItemCosmosDbEntity meetingNotificationItemTableEntity = new MeetingNotificationItemCosmosDbEntity();
            meetingNotificationItemTableEntity.Application = meetingNotificationItemEntity.Application;
            meetingNotificationItemTableEntity.Id = meetingNotificationItemEntity.Id;
            meetingNotificationItemTableEntity.PartitionKey = meetingNotificationItemEntity.Application;
            meetingNotificationItemTableEntity.RowKey = meetingNotificationItemEntity.NotificationId;
            meetingNotificationItemTableEntity.RequiredAttendees = meetingNotificationItemEntity.RequiredAttendees;
            meetingNotificationItemTableEntity.OptionalAttendees = meetingNotificationItemEntity.OptionalAttendees;
            meetingNotificationItemTableEntity.Application = meetingNotificationItemEntity.Application;
            meetingNotificationItemTableEntity.ErrorMessage = meetingNotificationItemEntity.ErrorMessage;
            meetingNotificationItemTableEntity.DayofMonth = meetingNotificationItemEntity.DayofMonth ?? default;
            meetingNotificationItemTableEntity.DayOfWeekByMonth = meetingNotificationItemEntity.DayOfWeekByMonth;
            meetingNotificationItemTableEntity.DaysOfWeek = meetingNotificationItemEntity.DaysOfWeek;
            meetingNotificationItemTableEntity.EndDate = meetingNotificationItemEntity.EndDate;
            meetingNotificationItemTableEntity.IsAllDayEvent = meetingNotificationItemEntity.IsAllDayEvent;
            meetingNotificationItemTableEntity.From = meetingNotificationItemEntity.From;
            meetingNotificationItemTableEntity.IsCancel = meetingNotificationItemEntity.IsCancel;
            meetingNotificationItemTableEntity.IsOnlineMeeting = meetingNotificationItemEntity.IsOnlineMeeting;
            meetingNotificationItemTableEntity.NotificationId = meetingNotificationItemEntity.NotificationId;
            meetingNotificationItemTableEntity.Priority = meetingNotificationItemEntity.Priority.ToString();
            meetingNotificationItemTableEntity.IsPrivate = meetingNotificationItemEntity.IsPrivate;
            meetingNotificationItemTableEntity.IsResponseRequested = meetingNotificationItemEntity.IsResponseRequested;
            meetingNotificationItemTableEntity.Status = meetingNotificationItemEntity.Status.ToString();
            meetingNotificationItemTableEntity.Subject = meetingNotificationItemEntity.Subject;
            meetingNotificationItemTableEntity.Location = meetingNotificationItemEntity.Location;
            meetingNotificationItemTableEntity.Timestamp = meetingNotificationItemEntity.Timestamp;
            meetingNotificationItemTableEntity.MonthOfYear = meetingNotificationItemEntity.MonthOfYear;
            meetingNotificationItemTableEntity.TrackingId = meetingNotificationItemEntity.TrackingId;
            meetingNotificationItemTableEntity.TryCount = meetingNotificationItemEntity.TryCount;
            meetingNotificationItemTableEntity.ETag = meetingNotificationItemEntity.ETag;
            meetingNotificationItemTableEntity.OccurrenceId = meetingNotificationItemEntity.OccurrenceId;
            meetingNotificationItemTableEntity.Ocurrences = meetingNotificationItemEntity.Ocurrences ?? default;
            meetingNotificationItemTableEntity.RecurrencePattern = meetingNotificationItemEntity.RecurrencePattern.ToString();
            meetingNotificationItemTableEntity.ReminderMinutesBeforeStart = meetingNotificationItemEntity.ReminderMinutesBeforeStart;
            meetingNotificationItemTableEntity.TemplateId = meetingNotificationItemEntity.TemplateId;
            meetingNotificationItemTableEntity.End = meetingNotificationItemEntity.End;
            meetingNotificationItemTableEntity.Start = meetingNotificationItemEntity.Start;
            meetingNotificationItemTableEntity.SequenceNumber = meetingNotificationItemEntity.SequenceNumber ?? default;
            meetingNotificationItemTableEntity.SendOnUtcDate = meetingNotificationItemEntity.SendOnUtcDate;
            meetingNotificationItemTableEntity.TrackingId = meetingNotificationItemEntity.TrackingId;
            meetingNotificationItemTableEntity.TryCount = meetingNotificationItemEntity.TryCount;
            meetingNotificationItemTableEntity.Timestamp = meetingNotificationItemEntity.Timestamp;
            meetingNotificationItemTableEntity.Interval = meetingNotificationItemEntity.Interval;
            meetingNotificationItemTableEntity.ICalUid = meetingNotificationItemEntity.ICalUid;
            meetingNotificationItemTableEntity.AttachmentReference = meetingNotificationItemEntity.AttachmentReference;
            meetingNotificationItemTableEntity.EmailAccountUsed = meetingNotificationItemEntity.EmailAccountUsed;
            meetingNotificationItemTableEntity.EventId = meetingNotificationItemEntity.EventId;
            meetingNotificationItemTableEntity.Action = meetingNotificationItemEntity.Action;
            return meetingNotificationItemTableEntity;
        }
    }
}
