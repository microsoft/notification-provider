// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.BusinessLibrary.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text;
    using NotificationService.BusinessLibrary.Business;
    using NotificationService.Contracts;
    using NotificationService.Contracts.Entities;

    /// <summary>
    /// MeetingInviteUtilities.
    /// </summary>
    public static class MeetingInviteUtilities
    {
        private const string StrTimeFormat = "yyyyMMdd\\THHmmss\\Z";

        /// <summary>
        /// Converts the meeting invite to body for direct send provider.
        /// </summary>
        /// <param name="meetingNotificationItem">The meeting notification item.</param>
        /// <param name="meetingBody">meetingBody.</param>
        /// <returns>A string for meeting invite.</returns>
        public static string ConvertDirectSendMeetingInviteToBody(MeetingNotificationItemEntity meetingNotificationItem, string meetingBody)
        {
            StringBuilder str = new StringBuilder();
#pragma warning disable IDE0058 // Expression value is never used
            str.AppendLine("BEGIN:VCALENDAR");

            str.AppendLine("PRODID:-//Microsoft Corporation//Outlook 14.0 MIMEDIR//EN");
            str.AppendLine("VERSION:2.0");

            // if (meetingNotificationItem.IsCancel)
            // {
            //    str.AppendLine("METHOD:CANCEL");
            //    str.AppendLine("STATUS:CANCELLED");
            // }
            // else
            // {
            str.AppendLine("METHOD:REQUEST");
            str.AppendLine("X-MS-OLK-FORCEINSPECTOROPEN:TRUE");
            str.AppendLine("BEGIN:VEVENT");
            str.AppendLine("DTSTART:" + meetingNotificationItem.Start.ToString(StrTimeFormat, CultureInfo.InvariantCulture));
            str.AppendLine("DTEND:" + meetingNotificationItem.End.ToString(StrTimeFormat, CultureInfo.InvariantCulture));

            // if (meetingNotificationItem.OccurenceId.HasValue) str.AppendLine("RECURRENCE-ID:" + invitation.OccurenceId.Value.ToString(c_strTimeFormat));
            str.AppendLine(GenerateRecurrenceRuleForSMTP(meetingNotificationItem));
            str.AppendLine(string.Format(CultureInfo.InvariantCulture, "UID:{0}", "icauid")); // currently harcoded as meeting invite is not being sent properly, if UID is null/empty
            str.AppendLine(string.Format(CultureInfo.InvariantCulture, "DESCRIPTION:{0}", meetingBody));
            str.AppendLine(string.Format(CultureInfo.InvariantCulture, "X-ALT-DESC;FMTTYPE=text/html:{0}", meetingBody));
            str.AppendLine(string.Format(CultureInfo.InvariantCulture, "SUMMARY:{0}", meetingNotificationItem.Subject));
            str.AppendLine(string.Format(CultureInfo.InvariantCulture, "LOCATION:{0}", meetingNotificationItem.Location));
            str.AppendLine(string.Format(CultureInfo.InvariantCulture, "ORGANIZER:MAILTO:{0}", meetingNotificationItem.From));
            str.AppendLine(string.Format(CultureInfo.InvariantCulture, "PRIORITY:{0}", GetMeetingPriority(meetingNotificationItem.Priority)));

            if (meetingNotificationItem.SequenceNumber.HasValue)
            {
                str.AppendLine(string.Format(CultureInfo.InvariantCulture, "SEQUENCE:{0}", meetingNotificationItem.SequenceNumber.Value));
            }

            if (meetingNotificationItem.IsPrivate)
            {
                str.AppendLine("CLASS:PRIVATE");
            }

            foreach (var to in meetingNotificationItem.RequiredAttendees?.Split(Common.ApplicationConstants.SplitCharacter, System.StringSplitOptions.RemoveEmptyEntries))
            {
                str.AppendLine(string.Format(CultureInfo.InvariantCulture, "ATTENDEE;PARTSTAT=NEEDS-ACTION;ROLE=REQ-PARTICIPANT;CN=\"{0}\";RSVP=FALSE:mailto:{0}", to));
            }

            if (!string.IsNullOrEmpty(meetingNotificationItem.OptionalAttendees))
            {
                foreach (var cc in meetingNotificationItem.OptionalAttendees?.Split(Common.ApplicationConstants.SplitCharacter, System.StringSplitOptions.RemoveEmptyEntries))
                {
                    str.AppendLine(string.Format(CultureInfo.InvariantCulture, "ATTENDEE;PARTSTAT=NEEDS-ACTION;ROLE=OPT-PARTICIPANT;CN=\"{0}\";RSVP=FALSE:mailto:{0}", cc));
                }
            }

            str.AppendLine("BEGIN:VALARM");
            if (!string.IsNullOrEmpty(meetingNotificationItem.ReminderMinutesBeforeStart))
            {
                str.AppendLine(string.Format(CultureInfo.InvariantCulture, "TRIGGER:-P{0}M", meetingNotificationItem.ReminderMinutesBeforeStart));
            }

            str.AppendLine("ACTION:DISPLAY");
            str.AppendLine("DESCRIPTION:Reminder");
            str.AppendLine("END:VALARM");
            str.AppendLine("END:VEVENT");
            str.AppendLine("END:VCALENDAR");

#pragma warning restore IDE0058 // Expression value is never used
            return str.ToString();
        }

        /// <summary>
        /// Converts the meeting invite to body for SMTP Provider.
        /// </summary>
        /// <param name="meetingNotificationItem">The meeting notification item.</param>
        /// <param name="meetingBody">meetingBody.</param>
        /// <param name="fromaddress">fromaddress.</param>
        /// <returns>A string for meeting invite.</returns>
        public static string ConvertSMTPMeetingInviteToBody(MeetingNotificationItemEntity meetingNotificationItem, string meetingBody, string fromaddress)
        {
            StringBuilder str = new StringBuilder();
#pragma warning disable IDE0058 // Expression value is never used
            str.AppendLine("BEGIN:VCALENDAR");

            str.AppendLine("PRODID:-//Microsoft Corporation//Outlook 14.0 MIMEDIR//EN");
            str.AppendLine("VERSION:2.0");

            if (meetingNotificationItem.IsCancel)
            {
                str.AppendLine("METHOD:CANCEL");
                str.AppendLine("STATUS:CANCELLED");
            }
            else
            {
                str.AppendLine("METHOD:REQUEST");
            }

            str.AppendLine("X-MS-OLK-FORCEINSPECTOROPEN:TRUE");
            str.AppendLine("BEGIN:VEVENT");
            str.AppendLine("DTSTART:" + meetingNotificationItem.Start.ToString(StrTimeFormat, CultureInfo.InvariantCulture));
            str.AppendLine("DTEND:" + meetingNotificationItem.End.ToString(StrTimeFormat, CultureInfo.InvariantCulture));

            // if (meetingNotificationItem.OccurenceId.HasValue) str.AppendLine("RECURRENCE-ID:" + invitation.OccurenceId.Value.ToString(c_strTimeFormat));
            str.AppendLine(GenerateRecurrenceRuleForSMTP(meetingNotificationItem));
            str.AppendLine(string.Format(CultureInfo.InvariantCulture, "UID:{0}", "icauid")); // currently harcoded as meeting invite is not being sent properly, if UID is null/empty
            str.AppendLine(string.Format(CultureInfo.InvariantCulture, "DESCRIPTION:{0}", meetingBody));
            str.AppendLine(string.Format(CultureInfo.InvariantCulture, "X-ALT-DESC;FMTTYPE=text/html:{0}", meetingBody));
            str.AppendLine(string.Format(CultureInfo.InvariantCulture, "SUMMARY:{0}", meetingNotificationItem.Subject));
            str.AppendLine(string.Format(CultureInfo.InvariantCulture, "LOCATION:{0}", meetingNotificationItem.Location));
            str.AppendLine(string.Format(CultureInfo.InvariantCulture, "ORGANIZER:MAILTO:{0}", fromaddress));
            str.AppendLine(string.Format(CultureInfo.InvariantCulture, "PRIORITY:{0}", GetMeetingPriority(meetingNotificationItem.Priority)));

            if (meetingNotificationItem.SequenceNumber.HasValue)
            {
                str.AppendLine(string.Format(CultureInfo.InvariantCulture, "SEQUENCE:{0}", meetingNotificationItem.SequenceNumber.Value));
            }

            if (meetingNotificationItem.IsPrivate)
            {
                str.AppendLine("CLASS:PRIVATE");
            }

            foreach (var attach in meetingNotificationItem.Attachments)
            {
                str.AppendLine(string.Format(CultureInfo.InvariantCulture, "ATTACH;ENCODING=BASE64;VALUE=BINARY;X-FILENAME={0}:{1}", attach.FileName, attach.FileBase64));
            }

            foreach (var to in meetingNotificationItem.RequiredAttendees?.Split(Common.ApplicationConstants.SplitCharacter, System.StringSplitOptions.RemoveEmptyEntries))
            {
                str.AppendLine(string.Format(CultureInfo.InvariantCulture, "ATTENDEE;PARTSTAT=NEEDS-ACTION;ROLE=REQ-PARTICIPANT;CN=\"{0}\";RSVP=FALSE:mailto:{0}", to));
            }

            if (!string.IsNullOrEmpty(meetingNotificationItem.OptionalAttendees))
            {
                foreach (var cc in meetingNotificationItem.OptionalAttendees?.Split(Common.ApplicationConstants.SplitCharacter, System.StringSplitOptions.RemoveEmptyEntries))
                {
                    str.AppendLine(string.Format(CultureInfo.InvariantCulture, "ATTENDEE;PARTSTAT=NEEDS-ACTION;ROLE=OPT-PARTICIPANT;CN=\"{0}\";RSVP=FALSE:mailto:{0}", cc));
                }
            }

            str.AppendLine("BEGIN:VALARM");
            if (!string.IsNullOrEmpty(meetingNotificationItem.ReminderMinutesBeforeStart))
            {
                str.AppendLine(string.Format(CultureInfo.InvariantCulture, "TRIGGER:-P{0}M", meetingNotificationItem.ReminderMinutesBeforeStart));
            }

            str.AppendLine("ACTION:DISPLAY");
            str.AppendLine("DESCRIPTION:Reminder");
            str.AppendLine("END:VALARM");
            str.AppendLine("END:VEVENT");
            str.AppendLine("END:VCALENDAR");

#pragma warning restore IDE0058 // Expression value is never used
            return str.ToString();
        }

        private static int GetMeetingPriority(NotificationPriority priority)
        {
            int meetingPriority = 0;
            switch (priority)
            {
                case NotificationPriority.High:
                    meetingPriority = 1;
                    break;
                case NotificationPriority.Low:
                    meetingPriority = 9;
                    break;
                default:
                    meetingPriority = 0;
                    break;
            }

            return meetingPriority;
        }

        private static string GenerateRecurrenceRuleForSMTP(MeetingNotificationItemEntity meetingNotificationItem)
        {
            switch (meetingNotificationItem.RecurrencePattern)
            {
                case MeetingRecurrencePattern.Daily:
                    return DailySMTP(meetingNotificationItem);
                case MeetingRecurrencePattern.Weekly:
                    return WeeklySMTP(meetingNotificationItem);
                case MeetingRecurrencePattern.Monthly:
                    return MonthlySMTP(meetingNotificationItem);
                case MeetingRecurrencePattern.Yearly:
                    return YearlySMTP(meetingNotificationItem);
                case MeetingRecurrencePattern.None:
                    break;
                default:
                    break;
            }

            return string.Empty;
        }

        private static string DailySMTP(MeetingNotificationItemEntity meetingNotificationItem)
        {
            if (meetingNotificationItem.EndDate.HasValue)
            {
                return string.Format(CultureInfo.InvariantCulture, "RRULE:FREQ=DAILY;INTERVAL={0};UNTIL={1}", meetingNotificationItem.Interval, meetingNotificationItem.EndDate.Value.ToString(StrTimeFormat, CultureInfo.InvariantCulture));
            }

            return string.Format(CultureInfo.InvariantCulture, "RRULE:FREQ=DAILY;INTERVAL={0};COUNT={1}", meetingNotificationItem.Interval, meetingNotificationItem.Ocurrences.HasValue ? meetingNotificationItem.Ocurrences.Value : 1); // Default to 1 Occurrence
        }

        private static string WeeklySMTP(MeetingNotificationItemEntity meetingNotificationItem)
        {
            if (meetingNotificationItem.EndDate.HasValue)
            {
                return string.Format(CultureInfo.InvariantCulture, "RRULE:FREQ=WEEKLY;BYDAY={0};INTERVAL={1};UNTIL={2}", DaysOfWeekToShortNames(GetDaysOfWeek(meetingNotificationItem.DaysOfWeek)), meetingNotificationItem.Interval, meetingNotificationItem.EndDate.Value.ToString(StrTimeFormat, CultureInfo.InvariantCulture));
            }

            return string.Format(CultureInfo.InvariantCulture, "RRULE:FREQ=WEEKLY;BYDAY={0};INTERVAL={1};COUNT={2}", DaysOfWeekToShortNames(GetDaysOfWeek(meetingNotificationItem.DaysOfWeek)), meetingNotificationItem.Interval, meetingNotificationItem.Ocurrences.HasValue ? meetingNotificationItem.Ocurrences.Value : 1); // Default to 1 Occurrence
        }

        private static string MonthlySMTP(MeetingNotificationItemEntity meetingNotificationItem)
        {
            StringBuilder sb = new StringBuilder();
            if (string.IsNullOrEmpty(meetingNotificationItem.DaysOfWeek))
            {
                _ = sb.Append(string.Format(CultureInfo.InvariantCulture, "RRULE:FREQ=MONTHLY;BYMONTHDAY={0};INTERVAL={1}", meetingNotificationItem.DayofMonth.HasValue ? meetingNotificationItem.DayofMonth.Value : meetingNotificationItem.Start.Day, meetingNotificationItem.Interval)); // If the Day of Month was not specified, use the day of the Start Date.
            }
            else
            {
                _ = sb.Append(string.Format(CultureInfo.InvariantCulture, "RRULE:FREQ=MONTHLY;BYSETPOS={0};BYDAY={1};INTERVAL={2}", GetDayOfWeekIndexSMTP(meetingNotificationItem.DayOfWeekByMonth), DaysOfWeekToShortNames(GetDaysOfWeek(meetingNotificationItem.DaysOfWeek)), meetingNotificationItem.Interval));
            }

            if (meetingNotificationItem.EndDate.HasValue)
            {
                _ = sb.Append(string.Format(CultureInfo.InvariantCulture, ";UNTIL={0}", meetingNotificationItem.EndDate.Value.ToString(StrTimeFormat, CultureInfo.InvariantCulture)));
            }
            else
            {
                _ = sb.Append(string.Format(CultureInfo.InvariantCulture, ";COUNT={0}", meetingNotificationItem.Ocurrences.HasValue ? meetingNotificationItem.Ocurrences.Value : 1)); // Default to 1 Occurrence
            }

            return sb.ToString();
        }

        private static string YearlySMTP(MeetingNotificationItemEntity meetingNotificationItem)
        {
            StringBuilder sb = new StringBuilder();

            if (string.IsNullOrEmpty(meetingNotificationItem.DaysOfWeek))
            {
                _ = sb.Append(string.Format(CultureInfo.InvariantCulture, "RRULE:FREQ=YEARLY;BYMONTH={0};BYMONTHDAY={1}", meetingNotificationItem.MonthOfYear, meetingNotificationItem.DayofMonth.HasValue ? meetingNotificationItem.DayofMonth.Value : meetingNotificationItem.Start.Day));
            }
            else
            {
                _ = sb.Append(string.Format(CultureInfo.InvariantCulture, "RRULE:FREQ=YEARLY;BYDAY={0};BYSETPOS={1};BYMONTH={2}", DaysOfWeekToShortNames(GetDaysOfWeek(meetingNotificationItem.DaysOfWeek)), GetDayOfWeekIndexSMTP(meetingNotificationItem.DayOfWeekByMonth), meetingNotificationItem.MonthOfYear));
            }

            if (meetingNotificationItem.EndDate.HasValue)
            {
                _ = sb.Append(string.Format(CultureInfo.InvariantCulture, ";UNTIL={0}", meetingNotificationItem.EndDate.Value.ToString(StrTimeFormat, CultureInfo.InvariantCulture)));
            }
            else
            {
                _ = sb.Append(string.Format(CultureInfo.InvariantCulture, ";COUNT={0}", meetingNotificationItem.Ocurrences.HasValue ? meetingNotificationItem.Ocurrences.Value : 1)); // Default to 1 Occurrence
            }

            return sb.ToString();
        }

        private static DayOfTheWeek[] GetDaysOfWeek(string daysOfWeek)
        {
            string[] splitDays = daysOfWeek.Split(',');
            List<DayOfTheWeek> days = new List<DayOfTheWeek>();

            for (int i = 0; i < splitDays.Length; i++)
            {
                days.Add((DayOfTheWeek)Enum.Parse(typeof(DayOfTheWeek), splitDays[i].Trim(), true));
            }

            return days.ToArray();
        }

        private static string GetDayOfWeekIndexSMTP(string dayOfMonthByWeek)
        {
            if (dayOfMonthByWeek.Trim() == "5")
            {
                return "-1"; // specific to SMTP
            }

            return dayOfMonthByWeek.Trim();
        }

        private static string DaysOfWeekToShortNames(DayOfTheWeek[] arr)
        {
            StringBuilder rtBldr = new StringBuilder();
            for (int i = 0; i < arr.Length; i++)
            {
                _ = rtBldr.Append(arr[i].ToString().Substring(0, 2).ToUpper(CultureInfo.InvariantCulture));
                _ = rtBldr.Append(",");
            }

            string retVal = rtBldr.ToString();
            return retVal.Length > 0 ? retVal.Substring(0, retVal.Length - 1) : string.Empty;
        }
    }
}
