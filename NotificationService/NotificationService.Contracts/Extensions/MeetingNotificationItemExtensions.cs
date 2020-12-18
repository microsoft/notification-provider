// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

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
                    DayofMonth = meetingNotificationItem.RecurrenceDayofMonth,
                    DayOfWeekByMonth = meetingNotificationItem.RecurrenceDayOfWeekByMonth,
                    DaysOfWeek = meetingNotificationItem.RecurrenceDaysOfWeek,
                    From = meetingNotificationItem.From,
                    EndDate = meetingNotificationItem.RecrurrenceEndDate,
                    End = meetingNotificationItem.MeetingEndTime,
                    Interval = meetingNotificationItem.RecurrenceInterval,
                    IsOnlineMeeting = meetingNotificationItem.IsOnlineMeeting,
                    IsResponseRequested = meetingNotificationItem.IsResponseRequested,
                    IsPrivate = meetingNotificationItem.IsPrivate,
                    OccurrenceId = meetingNotificationItem.OccurrenceId,
                    SequenceNumber = meetingNotificationItem.SequenceNumber,
                    Start = meetingNotificationItem.MeetingStartTime,
                    Subject = meetingNotificationItem.Subject,
                    ReminderMinutesBeforeStart = meetingNotificationItem.ReminderMinutesBeforeStart,
                    IsAllDayEvent = meetingNotificationItem.IsAllDayEvent,
                    IsCancel = meetingNotificationItem.IsCancel,
                    Location = meetingNotificationItem.Location,
                    MonthOfYear = meetingNotificationItem.RecurrenceMonthOfYear,
                    Ocurrences = meetingNotificationItem.NoOfMeetingOcurrences,
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

        /// <summary>
        /// Validates the meeting invite.
        /// </summary>
        /// <param name="meetingNotificationItem">The meeting notification item.</param>
        /// <returns>A <see cref="ValidationResult"/>.</returns>
        public static ValidationResult ValidateMeetingInvite(this MeetingNotificationItem meetingNotificationItem)
        {
            var res = new ValidationResult();
            if (meetingNotificationItem == null)
            {
                res.Result = true;
            }
            else if (meetingNotificationItem.RecurrencePattern == MeetingRecurrencePattern.Weekly && string.IsNullOrEmpty(meetingNotificationItem.RecurrenceDaysOfWeek))
            {
                res.Result = false;
                res.Message = $"RecurrenceDaysOfWeek cannot be null or empty, if RecurrencePattern is {MeetingRecurrencePattern.Weekly}";
            }
            else if (meetingNotificationItem.RecurrencePattern == MeetingRecurrencePattern.Monthly && !string.IsNullOrEmpty(meetingNotificationItem.RecurrenceDaysOfWeek) && string.IsNullOrEmpty(meetingNotificationItem.RecurrenceDayOfWeekByMonth))
            {
                res.Result = false;
                res.Message = $"RecurrenceDayOfWeekByMonth cannot be null or empty, if RecurrencePattern is {MeetingRecurrencePattern.Monthly} and RecurrenceDaysOfWeek is mentioned";
            }
            else if (meetingNotificationItem.RecurrencePattern == MeetingRecurrencePattern.Yearly && string.IsNullOrEmpty(meetingNotificationItem.RecurrenceDaysOfWeek) && meetingNotificationItem.RecurrenceMonthOfYear == 0)
            {
                res.Result = false;
                res.Message = $"RecurrenceMonthOfYear cannot be 0, if RecurrencePattern is {MeetingRecurrencePattern.Yearly} and RecurrenceDaysOfWeek is not mentioned";
            }
            else if (meetingNotificationItem.RecurrencePattern == MeetingRecurrencePattern.Yearly && !string.IsNullOrEmpty(meetingNotificationItem.RecurrenceDaysOfWeek) && (string.IsNullOrEmpty(meetingNotificationItem.RecurrenceDayOfWeekByMonth) || meetingNotificationItem.RecurrenceMonthOfYear == 0))
            {
                res.Result = false;
                res.Message = $"RecurrenceMonthOfYear and RecurrenceDayOfWeekByMonth cannot be null or empty, if RecurrencePattern is {MeetingRecurrencePattern.Yearly} and RecurrenceDaysOfWeek is mentioned";
            }
            else
            {
                res.Result = true;
            }

            return res;
        }
    }
}
