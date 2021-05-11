// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.BusinessLibrary.Providers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using DirectSend;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json;
    using NotificationService.BusinessLibrary.Business;
    using NotificationService.BusinessLibrary.Interfaces;
    using NotificationService.Common.Configurations;
    using NotificationService.Common.Logger;
    using NotificationService.Contracts;
    using NotificationService.Contracts.Entities;
    using static DirectSend.Models.Mail.EmailMessage;

    /// <summary>
    /// Direct Send Notification Provider.
    /// </summary>
    public class DirectSendNotificationProvider : INotificationProvider
    {
        /// <summary>
        /// Gets the MailSettings confiured.
        /// </summary>
        private readonly List<MailSettings> mailSettings;

        /// <summary>
        /// Instance of Application Configuration.
        /// </summary>
        private readonly IConfiguration configuration;

        /// <summary>
        /// Instance of <see cref="IEmailManager"/>.
        /// </summary>
        private readonly IEmailManager emailManager;

        /// <summary>
        /// Gets the DirectSendSetting confiured.
        /// </summary>
        private readonly DirectSendSetting directSendSetting;

        /// <summary>
        /// Instance of <see cref="IEmailService"/>.
        /// </summary>
        private readonly IEmailService mailService;

        /// <summary>
        /// Instance of <see cref="ILogger"/>.
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// The string time format.
        /// </summary>
        private readonly string strTimeFormat = "yyyyMMdd\\THHmmss\\Z";

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectSendNotificationProvider"/> class.
        /// </summary>
        /// <param name="configuration"> Instance of <see cref="IConfiguration"/>. </param>
        /// <param name="mailService">Instance of <see cref="IEmailService"/>.</param>
        /// <param name="logger">Instance of <see cref="ILogger"/>.</param>
        /// <param name="emailManager">Instance of <see cref="IEmailManager"/>.</param>
        public DirectSendNotificationProvider(
            IConfiguration configuration,
            IEmailService mailService,
            ILogger logger,
            IEmailManager emailManager)
        {
            this.configuration = configuration;
            this.mailService = mailService;
            this.logger = logger;
            if (this.configuration?[ConfigConstants.MailSettingsConfigKey] != null)
            {
                this.mailSettings = JsonConvert.DeserializeObject<List<MailSettings>>(this.configuration?[ConfigConstants
                    .MailSettingsConfigKey]);
            }

            this.emailManager = emailManager;
            this.directSendSetting = this.configuration.GetSection("DirectSendSetting").Get<DirectSendSetting>();
        }

        /// <inheritdoc/>
        public async Task ProcessMeetingNotificationEntities(string applicationName, IList<MeetingNotificationItemEntity> notificationEntities)
        {
            var traceprops = new Dictionary<string, string>();
            traceprops[AIConstants.Application] = applicationName;
            traceprops[AIConstants.MeetingNotificationCount] = notificationEntities?.Count.ToString(CultureInfo.InvariantCulture);

            this.logger.TraceInformation($"Started {nameof(this.ProcessMeetingNotificationEntities)} method of {nameof(DirectSendNotificationProvider)}.", traceprops);
            if (notificationEntities is null || notificationEntities.Count == 0)
            {
                throw new ArgumentNullException(nameof(notificationEntities), "notificationEntities are null.");
            }

            foreach (var item in notificationEntities)
            {
                item.TryCount++;
                item.ErrorMessage = string.Empty; // Reset the error message on next retry.
                try
                {
                    var sendForReal = this.mailSettings.Find(a => a.ApplicationName == applicationName).SendForReal;
                    var toOverride = this.mailSettings.Find(a => a.ApplicationName == applicationName).ToOverride;
                    DirectSend.Models.Mail.EmailMessage message = new DirectSend.Models.Mail.EmailMessage();
                    message.Subject = item.Subject;
                    MessageBody body = await this.emailManager.GetMeetingInviteBodyAsync(applicationName, item).ConfigureAwait(false);
                    message.FromAddresses = new List<DirectSend.Models.Mail.EmailAddress> { new DirectSend.Models.Mail.EmailAddress { Name = this.directSendSetting?.FromAddressDisplayName, Address = this.directSendSetting?.FromAddress } };
                    if (!sendForReal)
                    {
                        message.ToAddresses = toOverride.Split(Common.ApplicationConstants.SplitCharacter, System.StringSplitOptions.RemoveEmptyEntries)
                                 .Select(torecipient => new DirectSend.Models.Mail.EmailAddress { Address = torecipient }).ToList();
                        message.CcAddresses = null;
                    }
                    else
                    {
                        var toAddress = item.RequiredAttendees.Split(Common.ApplicationConstants.SplitCharacter, System.StringSplitOptions.RemoveEmptyEntries)
                                 .Select(torecipient => new DirectSend.Models.Mail.EmailAddress { Address = torecipient }).ToList();
                        if (item.OptionalAttendees?.Length > 0)
                        {
                            toAddress.AddRange(item.OptionalAttendees.Split(Common.ApplicationConstants.SplitCharacter, System.StringSplitOptions.RemoveEmptyEntries)?
                                     .Select(torecipient => new DirectSend.Models.Mail.EmailAddress { Address = torecipient }).ToList());
                        }

                        message.ToAddresses = toAddress;
                    }

                    message.FileName = item.Attachments?.Select(e => e.FileName);
                    message.FileContent = item.Attachments?.Select(e => e.FileBase64);
                    message.Content = this.ConvertMeetingInviteToBody(item, body.Content);
                    message.Importance = (ImportanceType)Enum.Parse(typeof(ImportanceType), item.Priority.ToString());
                    await this.mailService.SendMeetingInviteAsync(message).ConfigureAwait(false);
                    item.Status = NotificationItemStatus.Sent;
                }
#pragma warning disable CA1031 // Do not catch general exception types
                catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
                {
                    this.logger.WriteCustomEvent($"{AIConstants.CustomEventInviteSendFailed} for notificationId:  {item.NotificationId} ");
                    item.Status = NotificationItemStatus.Failed;
                    item.ErrorMessage = (ex.InnerException != null) ? ex.InnerException.Message : ex.Message;
                }

                this.logger.TraceInformation($"Finished {nameof(this.ProcessMeetingNotificationEntities)} method of {nameof(DirectSendNotificationProvider)}.", traceprops);
            }
        }

        /// <inheritdoc/>
        public async Task ProcessNotificationEntities(string applicationName, IList<EmailNotificationItemEntity> notificationEntities)
        {
            var traceprops = new Dictionary<string, string>();
            traceprops[AIConstants.Application] = applicationName;
            traceprops[AIConstants.MeetingNotificationCount] = notificationEntities?.Count.ToString(CultureInfo.InvariantCulture);

            this.logger.TraceInformation($"Started {nameof(this.ProcessNotificationEntities)} method of {nameof(DirectSendNotificationProvider)}.", traceprops);
            if (notificationEntities is null || notificationEntities.Count == 0)
            {
                throw new ArgumentNullException(nameof(notificationEntities), "notificationEntities are null.");
            }

            foreach (var item in notificationEntities)
            {
                item.EmailAccountUsed = item.From;
                item.TryCount++;
                item.ErrorMessage = string.Empty; // Reset the error message on next retry.
                try
                {
                    var sendForReal = this.mailSettings.Find(a => a.ApplicationName == applicationName).SendForReal;
                    var toOverride = this.mailSettings.Find(a => a.ApplicationName == applicationName).ToOverride;
                    MessageBody body = await this.emailManager.GetNotificationMessageBodyAsync(applicationName, item).ConfigureAwait(false);
                    DirectSend.Models.Mail.EmailMessage message = item.ToDirectSendEmailMessage(body, this.directSendSetting);
                    if (!sendForReal)
                    {
                        message.ToAddresses = toOverride.Split(Common.ApplicationConstants.SplitCharacter, System.StringSplitOptions.RemoveEmptyEntries)
                                 .Select(torecipient => new DirectSend.Models.Mail.EmailAddress { Address = torecipient }).ToList();
                        message.CcAddresses = null;
                    }

                    await this.mailService.SendEmailAsync(message).ConfigureAwait(false);
                    item.Status = NotificationItemStatus.Sent;
                }
#pragma warning disable CA1031 // Do not catch general exception types
                catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
                {
                    this.logger.WriteCustomEvent($"{AIConstants.CustomEventMailSendFailed} for notificationId:  {item.NotificationId} ");
                    item.Status = NotificationItemStatus.Failed;
                    item.ErrorMessage = (ex.InnerException != null) ? ex.InnerException.Message : ex.Message;
                }
            }

            this.logger.TraceInformation($"Finished {nameof(this.ProcessNotificationEntities)} method of {nameof(DirectSendNotificationProvider)}.", traceprops);
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

        /// <summary>
        /// Converts the meeting invite to body.
        /// </summary>
        /// <param name="meetingNotificationItem">The meeting notification item.</param>
        /// <param name="meetingBody">meetingBody.</param>
        /// <returns>A string for meeting invite.</returns>
        private string ConvertMeetingInviteToBody(MeetingNotificationItemEntity meetingNotificationItem, string meetingBody)
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
            str.AppendLine("DTSTART:" + meetingNotificationItem.Start.ToString(this.strTimeFormat, CultureInfo.InvariantCulture));
            str.AppendLine("DTEND:" + meetingNotificationItem.End.ToString(this.strTimeFormat, CultureInfo.InvariantCulture));

            // if (meetingNotificationItem.OccurenceId.HasValue) str.AppendLine("RECURRENCE-ID:" + invitation.OccurenceId.Value.ToString(c_strTimeFormat));
            str.AppendLine(this.GenerateRecurrenceRuleForSMTP(meetingNotificationItem));
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

        private string GenerateRecurrenceRuleForSMTP(MeetingNotificationItemEntity meetingNotificationItem)
        {
            switch (meetingNotificationItem.RecurrencePattern)
            {
                case MeetingRecurrencePattern.Daily:
                    return this.DailySMTP(meetingNotificationItem);
                case MeetingRecurrencePattern.Weekly:
                    return this.WeeklySMTP(meetingNotificationItem);
                case MeetingRecurrencePattern.Monthly:
                    return this.MonthlySMTP(meetingNotificationItem);
                case MeetingRecurrencePattern.Yearly:
                    return this.YearlySMTP(meetingNotificationItem);
                case MeetingRecurrencePattern.None:
                    break;
                default:
                    break;
            }

            return string.Empty;
        }

        private string DailySMTP(MeetingNotificationItemEntity meetingNotificationItem)
        {
            if (meetingNotificationItem.EndDate.HasValue)
            {
                return string.Format(CultureInfo.InvariantCulture, "RRULE:FREQ=DAILY;INTERVAL={0};UNTIL={1}", meetingNotificationItem.Interval, meetingNotificationItem.EndDate.Value.ToString(this.strTimeFormat, CultureInfo.InvariantCulture));
            }

            return string.Format(CultureInfo.InvariantCulture, "RRULE:FREQ=DAILY;INTERVAL={0};COUNT={1}", meetingNotificationItem.Interval, meetingNotificationItem.Ocurrences.HasValue ? meetingNotificationItem.Ocurrences.Value : 1); // Default to 1 Occurrence
        }

        private string WeeklySMTP(MeetingNotificationItemEntity meetingNotificationItem)
        {
            if (meetingNotificationItem.EndDate.HasValue)
            {
                return string.Format(CultureInfo.InvariantCulture, "RRULE:FREQ=WEEKLY;BYDAY={0};INTERVAL={1};UNTIL={2}", DaysOfWeekToShortNames(GetDaysOfWeek(meetingNotificationItem.DaysOfWeek)), meetingNotificationItem.Interval, meetingNotificationItem.EndDate.Value.ToString(this.strTimeFormat, CultureInfo.InvariantCulture));
            }

            return string.Format(CultureInfo.InvariantCulture, "RRULE:FREQ=WEEKLY;BYDAY={0};INTERVAL={1};COUNT={2}", DaysOfWeekToShortNames(GetDaysOfWeek(meetingNotificationItem.DaysOfWeek)), meetingNotificationItem.Interval, meetingNotificationItem.Ocurrences.HasValue ? meetingNotificationItem.Ocurrences.Value : 1); // Default to 1 Occurrence
        }

        private string MonthlySMTP(MeetingNotificationItemEntity meetingNotificationItem)
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
                _ = sb.Append(string.Format(CultureInfo.InvariantCulture, ";UNTIL={0}", meetingNotificationItem.EndDate.Value.ToString(this.strTimeFormat, CultureInfo.InvariantCulture)));
            }
            else
            {
                _ = sb.Append(string.Format(CultureInfo.InvariantCulture, ";COUNT={0}", meetingNotificationItem.Ocurrences.HasValue ? meetingNotificationItem.Ocurrences.Value : 1)); // Default to 1 Occurrence
            }

            return sb.ToString();
        }

        private string YearlySMTP(MeetingNotificationItemEntity meetingNotificationItem)
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
                _ = sb.Append(string.Format(CultureInfo.InvariantCulture, ";UNTIL={0}", meetingNotificationItem.EndDate.Value.ToString(this.strTimeFormat, CultureInfo.InvariantCulture)));
            }
            else
            {
                _ = sb.Append(string.Format(CultureInfo.InvariantCulture, ";COUNT={0}", meetingNotificationItem.Ocurrences.HasValue ? meetingNotificationItem.Ocurrences.Value : 1)); // Default to 1 Occurrence
            }

            return sb.ToString();
        }
    }
}