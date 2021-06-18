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
    using NotificationService.BusinessLibrary.Utilities;
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
                    message.Content = MeetingInviteUtilities.ConvertDirectSendMeetingInviteToBody(item, body.Content);
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
    }
}