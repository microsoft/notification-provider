// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.BusinessLibrary.Providers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using DirectSend;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json;
    using NotificationService.BusinessLibrary.Interfaces;
    using NotificationService.Common.Configurations;
    using NotificationService.Common.Logger;
    using NotificationService.Contracts;

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
                    }

                    await this.mailService.SendAsync(message).ConfigureAwait(false);
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
    }
}