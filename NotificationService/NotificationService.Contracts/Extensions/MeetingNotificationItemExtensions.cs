namespace NotificationService.Contracts.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using NotificationService.Common.Encryption;
    using NotificationService.Contracts.Entities;
    using NotificationService.Contracts.Models;

    /// <summary>
    /// MeetingNotificationItemExtensions.
    /// </summary>
    public static class MeetingNotificationItemExtensions
    {
        /// <summary>
        /// Converts to entity.
        /// </summary>
        /// <param name="meetingNotificationItem">The meeting notification item.</param>
        /// <param name="applicationName">Name of the application.</param>
        /// <param name="encryptionService">encryptionService.</param>
        /// <returns>A <see cref="MeetingNotificationItemEntity"/>.</returns>
        public static MeetingNotificationItemEntity ToEntity(this MeetingNotificationItem meetingNotificationItem, string applicationName, IEncryptionService encryptionService)
        {
            if (meetingNotificationItem != null)
            {
                return new MeetingNotificationItemEntity
                {
                    Attachments = meetingNotificationItem.Attachments,
                    ICalUid = meetingNotificationItem.ICalUid,
                    OptionalAttendees = meetingNotificationItem.OptionalAttendees,
                    RequiredAttendees = meetingNotificationItem.RequiredAttendees,
                    Body = meetingNotificationItem?.Body != null ? encryptionService.Encrypt(meetingNotificationItem?.Body) : null,
                    NotificationId = meetingNotificationItem.NotificationId,
                    DayofMonth = meetingNotificationItem.DayofMonth,
                    DayOfWeekByMonth = meetingNotificationItem.DayOfWeekByMonth,
                    DaysOfWeek = meetingNotificationItem.DaysOfWeek,
                    From = meetingNotificationItem.From,
                    EndDate = meetingNotificationItem.EndDate,
                    End = meetingNotificationItem.End,
                    Interval = meetingNotificationItem.Interval,
                    IsOnlineMeeting = meetingNotificationItem.IsOnlineMeeting,
                    IsResponseRequested = meetingNotificationItem.IsResponseRequested,
                    IsPrivate = meetingNotificationItem.IsPrivate,
                    OccurrenceId = meetingNotificationItem.OccurrenceId,
                    SequenceNumber = meetingNotificationItem.SequenceNumber,
                    Start = meetingNotificationItem.Start,
                    Subject = meetingNotificationItem.Subject,
                    ReminderMinutesBeforeStart = meetingNotificationItem.ReminderMinutesBeforeStart,
                    IsAllDayEvent = meetingNotificationItem.IsAllDayEvent,
                    IsCancel = meetingNotificationItem.IsCancel,
                    Location = meetingNotificationItem.Location,
                    MonthOfYear = meetingNotificationItem.MonthOfYear,
                    Ocurrences = meetingNotificationItem.Ocurrences,
                    Priority = meetingNotificationItem.Priority,
                    RecurrencePattern = meetingNotificationItem.RecurrencePattern,
                    TemplateName = meetingNotificationItem.TemplateName,
                    TemplateData = meetingNotificationItem.TemplateData,
                    Application = applicationName,
                };
            }
            else
            {
                return null;
            }
        }
    }
}
