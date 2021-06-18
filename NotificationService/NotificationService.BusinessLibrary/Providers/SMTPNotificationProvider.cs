// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.BusinessLibrary.Providers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Mail;
    using System.Net.Mime;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json;
    using NotificationService.BusinessLibrary.Interfaces;
    using NotificationService.BusinessLibrary.Utilities;
    using NotificationService.Common.Configurations;
    using NotificationService.Common.Logger;
    using NotificationService.Contracts;
    using NotificationService.Contracts.Entities;

    /// <summary>
    /// SMTP Notification Provider.
    /// </summary>
    public class SMTPNotificationProvider : INotificationProvider
    {
        /// <summary>
        /// Instance of <see cref="IEmailAccountManager"/>.
        /// </summary>
        private readonly IEmailAccountManager emailAccountManager;

        /// <summary>
        /// Instance of <see cref="ILogger"/>.
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// SMTP configuration.
        /// </summary>
        private readonly SMTPSetting smtpSetting;

        /// <summary>
        /// Instance of Application Configuration.
        /// </summary>
        private readonly IConfiguration configuration;

        /// <summary>
        /// List of Application to Account details mappings.
        /// </summary>
        private readonly List<ApplicationAccounts> applicationAccounts;

        /// <summary>
        /// The Max Retry count allowed for an item.
        /// </summary>
        private readonly int maxTryCount;

        /// <summary>
        /// Gets the MailSettings confiured.
        /// </summary>
        private readonly List<MailSettings> mailSettings;

        /// <summary>
        /// Instance of <see cref="IEmailManager"/>.
        /// </summary>
        private readonly IEmailManager emailManager;


        /// <summary>
        /// Initializes a new instance of the <see cref="SMTPNotificationProvider"/> class.
        /// </summary>
        /// <param name="emailAccountManager"> IEmailAccountManager.</param>
        /// <param name="logger"> ILogger.</param>
        /// <param name="configuration"> IConfiguration.</param>
        /// <param name="emailManager"> IEmailManager.</param>
        public SMTPNotificationProvider(
             IEmailAccountManager emailAccountManager,
             ILogger logger,
             IConfiguration configuration,
             IEmailManager emailManager)
        {
            this.emailAccountManager = emailAccountManager;
            this.logger = logger;
            this.configuration = configuration;
            this.smtpSetting = this.configuration?.GetSection(ConfigConstants.SMTPSettingConfigSectionKey).Get<SMTPSetting>();
            this.applicationAccounts = JsonConvert.DeserializeObject<List<ApplicationAccounts>>(this.configuration?[ConfigConstants.ApplicationAccountsConfigSectionKey]);
            _ = int.TryParse(this.configuration[$"{ConfigConstants.RetrySettingConfigSectionKey}:{ConfigConstants.RetrySettingMaxRetryCountConfigKey}"], out this.maxTryCount);
            if (this.configuration?[ConfigConstants.MailSettingsConfigKey] != null)
            {
                this.mailSettings = JsonConvert.DeserializeObject<List<MailSettings>>(this.configuration?[ConfigConstants.MailSettingsConfigKey]);
            }
            this.emailManager = emailManager;
        }

        /// <inheritdoc/>
        public async Task ProcessMeetingNotificationEntities(string applicationName, IList<MeetingNotificationItemEntity> notificationEntities)
        {
            var traceprops = new Dictionary<string, string>();
            traceprops[AIConstants.Application] = applicationName;
            traceprops[AIConstants.MeetingNotificationCount] = notificationEntities?.Count.ToString(CultureInfo.InvariantCulture);

            this.logger.TraceInformation($"Started {nameof(this.ProcessMeetingNotificationEntities)} method of {nameof(SMTPNotificationProvider)}.", traceprops);

            if (notificationEntities is null || notificationEntities.Count == 0)
            {
                throw new ArgumentNullException(nameof(notificationEntities), "notificationEntities are null.");
            }
            var client = new SmtpClient(this.smtpSetting.SmtpUrl, this.smtpSetting.SmtpPort);

            AccountCredential selectedAccountCreds = this.emailAccountManager.FetchAccountToBeUsedForApplication(applicationName, this.applicationAccounts);
            client.UseDefaultCredentials = false;
            client.Credentials = new NetworkCredential(selectedAccountCreds.AccountName, selectedAccountCreds.PrimaryPassword, this.smtpSetting.SmtpDomain);
            client.EnableSsl = true;
            var applicationFromAddress = this.applicationAccounts.Find(a => a.ApplicationName == applicationName).FromOverride;

            foreach (var item in notificationEntities)
            {
                item.TryCount++;
                item.ErrorMessage = string.Empty; // Reset the error message on next retry.
                try
                {
                    var sendForReal = this.mailSettings.Find(a => a.ApplicationName == applicationName).SendForReal;
                    var toOverride = this.mailSettings.Find(a => a.ApplicationName == applicationName).ToOverride;
                    MessageBody body = await this.emailManager.GetMeetingInviteBodyAsync(applicationName, item).ConfigureAwait(false);
                    MailMessage message = new MailMessage();
                    message.Subject = item.Subject;
                    message.From = new MailAddress(applicationFromAddress);
                    if (!sendForReal)
                    {
                        message.To.Add(string.Join(",", toOverride.Split(Common.ApplicationConstants.SplitCharacter, System.StringSplitOptions.RemoveEmptyEntries).ToList()));
                        message.CC.Clear();
                        message.Bcc.Clear();
                        message.ReplyToList.Clear();
                    }
                    else
                    {
                        if (item.RequiredAttendees?.Length > 0)
                        {
                            message.To.Add(string.Join(",", item.RequiredAttendees.Split(Common.ApplicationConstants.SplitCharacter, System.StringSplitOptions.RemoveEmptyEntries).ToList()));
                        }

                        if (item.OptionalAttendees?.Length > 0)
                        {
                            message.CC.Add(string.Join(",", item.RequiredAttendees.Split(Common.ApplicationConstants.SplitCharacter, System.StringSplitOptions.RemoveEmptyEntries).ToList()));
                        }
                    }


                    message.Priority = (MailPriority)Enum.Parse(typeof(MailPriority), item.Priority.ToString());

                    ContentType ctBody = new ContentType("text/html");
                    AlternateView viewBody = AlternateView.CreateAlternateViewFromString(body.Content, ctBody);
                    message.AlternateViews.Add(viewBody);

                    string str = MeetingInviteUtilities.ConvertSMTPMeetingInviteToBody(item, body.Content, applicationFromAddress);

                    ContentType ct = new ContentType("text/calendar");
                    if (item.IsCancel)
                    {
                        ct.Parameters.Add("method", "CANCEL");
                    }
                    else
                    {
                        ct.Parameters.Add("method", "REQUEST");
                    }

                    AlternateView avCal = AlternateView.CreateAlternateViewFromString(str, ct);

                    message.AlternateViews.Add(avCal);

                    await client.SendMailAsync(message).ConfigureAwait(false);

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

                this.logger.TraceInformation($"Finished {nameof(this.ProcessMeetingNotificationEntities)} method of {nameof(SMTPNotificationProvider)}.", traceprops);
            }
        }

        /// <inheritdoc/>
        public async Task ProcessNotificationEntities(string applicationName, IList<EmailNotificationItemEntity> notificationEntities)
        {
            var traceprops = new Dictionary<string, string>();
            traceprops[AIConstants.Application] = applicationName;
            traceprops[AIConstants.MeetingNotificationCount] = notificationEntities?.Count.ToString(CultureInfo.InvariantCulture);

            this.logger.TraceInformation($"Started {nameof(this.ProcessNotificationEntities)} method of {nameof(SMTPNotificationProvider)}.", traceprops);
            if (notificationEntities is null || notificationEntities.Count == 0)
            {
                throw new ArgumentNullException(nameof(notificationEntities), "notificationEntities are null.");
            }

            var client = new SmtpClient(this.smtpSetting.SmtpUrl, this.smtpSetting.SmtpPort);

            AccountCredential selectedAccountCreds = this.emailAccountManager.FetchAccountToBeUsedForApplication(applicationName, this.applicationAccounts);
            client.UseDefaultCredentials = false;
            client.Credentials = new NetworkCredential(selectedAccountCreds.AccountName, selectedAccountCreds.PrimaryPassword, this.smtpSetting.SmtpDomain);
            client.EnableSsl = true;
            var applicationFromAddress = this.applicationAccounts.Find(a => a.ApplicationName == applicationName).FromOverride;

            foreach (var item in notificationEntities)
            {
                item.From = applicationFromAddress;
                item.TryCount++;
                item.ErrorMessage = string.Empty; // Reset the error message on next retry.
                try
                {
                    var sendForReal = this.mailSettings.Find(a => a.ApplicationName == applicationName).SendForReal;
                    var toOverride = this.mailSettings.Find(a => a.ApplicationName == applicationName).ToOverride;
                    MessageBody body = await this.emailManager.GetNotificationMessageBodyAsync(applicationName, item).ConfigureAwait(false);
                    MailMessage message = item.ToSmtpMailMessage(body);
                    if (!sendForReal)
                    {
                        message.To.Add(string.Join(",", toOverride.Split(Common.ApplicationConstants.SplitCharacter, System.StringSplitOptions.RemoveEmptyEntries).ToList()));
                        message.CC.Clear();
                        message.Bcc.Clear();
                        message.ReplyToList.Clear();
                    }

                    await client.SendMailAsync(message).ConfigureAwait(false);
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

            this.logger.TraceInformation($"Finished {nameof(this.ProcessNotificationEntities)} method of {nameof(SMTPNotificationProvider)}.", traceprops);
        }
    }
}
