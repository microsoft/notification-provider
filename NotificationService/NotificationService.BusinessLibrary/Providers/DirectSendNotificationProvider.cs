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
    using MimeKit;
    using Newtonsoft.Json;
    using NotificationService.BusinessLibrary.Interfaces;
    using NotificationService.Common.Configurations;
    using NotificationService.Common.Logger;
    using NotificationService.Contracts;
    using NotificationService.Contracts.Entities;

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
            if (this.configuration?["MailSettings"] != null)
            {
                this.mailSettings = JsonConvert.DeserializeObject<List<MailSettings>>(this.configuration?["MailSettings"]);
            }

            this.emailManager = emailManager;
            this.directSendSetting = this.configuration.GetSection("DirectSendSetting").Get<DirectSendSetting>();
        }

        /// <inheritdoc/>
        public async Task ProcessMeetingNotificationEntities(string applicationName, IList<MeetingNotificationItemEntity> notificationEntities)
        {
            this.logger.TraceInformation($"Started {nameof(this.ProcessMeetingNotificationEntities)} method of {nameof(DirectSendNotificationProvider)}.");
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
                    if (!sendForReal)
                    {
                        message.ToAddresses = toOverride.Split(Common.Constants.SplitCharacter, System.StringSplitOptions.RemoveEmptyEntries)
                                 .Select(torecipient => new DirectSend.Models.Mail.EmailAddress { Address = torecipient }).ToList();
                        message.CcAddresses = null;
                    }
                    else
                    {
                        var toAddress = item.RequiredAttendees.Split(Common.Constants.SplitCharacter, System.StringSplitOptions.RemoveEmptyEntries)
                                 .Select(torecipient => new DirectSend.Models.Mail.EmailAddress { Address = torecipient }).ToList();
                        toAddress.AddRange(item.OptionalAttendees.Split(Common.Constants.SplitCharacter, System.StringSplitOptions.RemoveEmptyEntries)
                                 .Select(torecipient => new DirectSend.Models.Mail.EmailAddress { Address = torecipient }).ToList());
                        message.ToAddresses = toAddress;
                    }

                    message.Content = this.ConvertMeetingInviteToBody(item);
                    await this.mailService.SendMeetingInviteAsync(message).ConfigureAwait(false);
                    item.Status = NotificationItemStatus.Sent;
                }
                catch (Exception ex)
                {
                    item.Status = NotificationItemStatus.Failed;
                    item.ErrorMessage = (ex.InnerException != null) ? ex.InnerException.Message : ex.Message;
                }
            }
        }

        /// <inheritdoc/>
        public async Task ProcessNotificationEntities(string applicationName, IList<EmailNotificationItemEntity> notificationEntities)
        {
            this.logger.TraceInformation($"Started {nameof(this.ProcessNotificationEntities)} method of {nameof(DirectSendNotificationProvider)}.");
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
                        message.ToAddresses = toOverride.Split(Common.Constants.SplitCharacter, System.StringSplitOptions.RemoveEmptyEntries)
                                 .Select(torecipient => new DirectSend.Models.Mail.EmailAddress { Address = torecipient }).ToList();
                        message.CcAddresses = null;
                    }

                    await this.mailService.SendEmailAsync(message).ConfigureAwait(false);
                    item.Status = NotificationItemStatus.Sent;
                }
                catch (Exception ex)
                {
                    item.Status = NotificationItemStatus.Failed;
                    item.ErrorMessage = (ex.InnerException != null) ? ex.InnerException.Message : ex.Message;
                }
            }

            this.logger.TraceInformation($"Finished {nameof(this.ProcessNotificationEntities)} method of {nameof(DirectSendNotificationProvider)}.");
        }

        /// <summary>
        /// Converts the meeting invite to body.
        /// </summary>
        /// <param name="meetingNotificationItem">The meeting notification item.</param>
        /// <returns>A string for meeting invite</returns>
        private string ConvertMeetingInviteToBody(MeetingNotificationItemEntity meetingNotificationItem)
        {
            StringBuilder str = new StringBuilder();
#pragma warning disable IDE0058 // Expression value is never used
            str.AppendLine("BEGIN:VCALENDAR");

            str.AppendLine("PRODID:-//Microsoft Corporation//Outlook 14.0 MIMEDIR//EN"); //
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
            str.AppendLine("DTSTART:" + meetingNotificationItem.Start.ToString("yyyyMMdd\\THHmmss\\Z", CultureInfo.InvariantCulture));
            str.AppendLine("DTEND:" + meetingNotificationItem.End.ToString("yyyyMMdd\\THHmmss\\Z", CultureInfo.InvariantCulture));

            // if (meetingNotificationItem.OccurenceId.HasValue) str.AppendLine("RECURRENCE-ID:" + invitation.OccurenceId.Value.ToString(c_strTimeFormat));
            // str.AppendLine(GenerateRecurrenceRuleForSMTP(invitation));
            // str.AppendLine(string.Format("UID:{0}", invitation.ICalUid));
            str.AppendLine(string.Format(CultureInfo.InvariantCulture, "DESCRIPTION:{0}", meetingNotificationItem.Body));
            str.AppendLine(string.Format(CultureInfo.InvariantCulture, "X-ALT-DESC;FMTTYPE=text/html:{0}", meetingNotificationItem.Body));
            str.AppendLine(string.Format(CultureInfo.InvariantCulture, "SUMMARY:{0}", meetingNotificationItem.Subject));
            str.AppendLine(string.Format(CultureInfo.InvariantCulture, "LOCATION:{0}", meetingNotificationItem.Location));
            str.AppendLine(string.Format(CultureInfo.InvariantCulture, "ORGANIZER:MAILTO:{0}", meetingNotificationItem.From));
            // if (invitation.SequenceNumber.HasValue) str.AppendLine(string.Format("SEQUENCE:{0}", invitation.SequenceNumber.Value));

            if (meetingNotificationItem.IsPrivate)
            {
                str.AppendLine("CLASS:PRIVATE");
            }

            foreach (var to in meetingNotificationItem.RequiredAttendees?.Split(Common.Constants.SplitCharacter, System.StringSplitOptions.RemoveEmptyEntries))
            {
                str.AppendLine(string.Format(CultureInfo.InvariantCulture, "ATTENDEE;PARTSTAT=NEEDS-ACTION;ROLE=REQ-PARTICIPANT;RSVP=FALSE:mailto:{0}", to));
            }

            if (!string.IsNullOrEmpty(meetingNotificationItem.OptionalAttendees))
            {
                foreach (var cc in meetingNotificationItem.OptionalAttendees?.Split(Common.Constants.SplitCharacter, System.StringSplitOptions.RemoveEmptyEntries))
                {
                    str.AppendLine(string.Format(CultureInfo.InvariantCulture, "ATTENDEE;PARTSTAT=NEEDS-ACTION;ROLE=OPT-PARTICIPANT;RSVP=FALSE:mailto:{0}", cc));
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
    }
}