﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.BusinessLibrary.Providers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json;
    using NotificationService.BusinessLibrary.Interfaces;
    using NotificationService.Common;
    using NotificationService.Common.Configurations;
    using NotificationService.Common.Logger;
    using NotificationService.Common.Utility;
    using NotificationService.Contracts;
    using NotificationService.Contracts.Entities;

    /// <summary>
    /// MS Graph Notification Provider.
    /// </summary>
    public class MSGraphNotificationProvider : INotificationProvider
    {
        private readonly IEmailAccountManager emailAccountManager;

        /// <summary>
        /// Instance of <see cref="ILogger"/>.
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// MS Graph configuration.
        /// </summary>
        private readonly MSGraphSetting mSGraphSetting;

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
        /// Polly Retry Setting.
        /// </summary>
        private readonly RetrySetting pollyRetrySetting;

        /// <summary>
        /// JSON serializer settings for http calls.
        /// </summary>
        private readonly JsonSerializerSettings jsonSerializerSettings;

        /// <summary>
        /// Instance of <see cref="ITokenHelper"/>.
        /// </summary>
        private readonly ITokenHelper tokenHelper;

        /// <summary>
        /// Instance of <see cref="IMSGraphProvider"/>.
        /// </summary>
        private readonly IMSGraphProvider msGraphProvider;

        /// <summary>
        /// Instance of <see cref="IEmailManager"/>.
        /// </summary>
        private readonly IEmailManager emailManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="MSGraphNotificationProvider"/> class.
        /// </summary>
        /// <param name="configuration">Instance of <see cref="IConfiguration"/>.</param>
        /// <param name="emailAccountManager">Instance of <see cref="IEmailAccountManager"/>.</param>
        /// <param name="logger">Instance of <see cref="ILogger"/>.</param>
        /// <param name="mSGraphSetting">Instance of <see cref="IEmailManager"/>.</param>
        /// <param name="pollyRetrySetting">Instance of <see cref="RetrySetting"/>.</param>
        /// <param name="tokenHelper">Instance of <see cref="ITokenHelper"/>.</param>
        /// <param name="msGraphProvider">Instance of <see cref="IMSGraphProvider"/>.</param>
        /// <param name="emailManager">Instance of <see cref="IEmailManager"/>..</param>
        public MSGraphNotificationProvider(
             IConfiguration configuration,
             IEmailAccountManager emailAccountManager,
             ILogger logger,
             IOptions<MSGraphSetting> mSGraphSetting,
             IOptions<RetrySetting> pollyRetrySetting,
             ITokenHelper tokenHelper,
             IMSGraphProvider msGraphProvider,
             IEmailManager emailManager)
        {
            this.configuration = configuration;
            this.applicationAccounts = JsonConvert.DeserializeObject<List<ApplicationAccounts>>(this.configuration?["ApplicationAccounts"]);
            this.emailAccountManager = emailAccountManager;
            this.logger = logger;
            _ = int.TryParse(this.configuration["RetrySetting:MaxRetries"], out this.maxTryCount);
            if (this.configuration?["MailSettings"] != null)
            {
                this.mailSettings = JsonConvert.DeserializeObject<List<MailSettings>>(this.configuration?["MailSettings"]);
            }

            this.mSGraphSetting = mSGraphSetting?.Value;
            this.pollyRetrySetting = pollyRetrySetting?.Value;
            this.jsonSerializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver(),
                NullValueHandling = NullValueHandling.Ignore,
            };
            this.tokenHelper = tokenHelper;
            this.msGraphProvider = msGraphProvider;
            this.emailManager = emailManager;
        }

        /// <inheritdoc/>
        public async Task ProcessNotificationEntities(string applicationName, IList<EmailNotificationItemEntity> notificationEntities)
         {
            this.logger.TraceInformation($"Started {nameof(this.ProcessNotificationEntities)} method of {nameof(MSGraphNotificationProvider)}.");
            if (notificationEntities is null || notificationEntities.Count == 0)
            {
                throw new ArgumentNullException(nameof(notificationEntities), "notificationEntities are null.");
            }

            var traceProps = new Dictionary<string, string>();
            traceProps[Constants.Application] = applicationName;
            traceProps[Constants.EmailNotificationCount] = notificationEntities.Count.ToString(CultureInfo.InvariantCulture);

            var applicationFromAddress = this.applicationAccounts.Find(a => a.ApplicationName == applicationName).FromOverride;
            AccountCredential selectedAccountCreds = this.emailAccountManager.FetchAccountToBeUsedForApplication(applicationName, this.applicationAccounts);
            AuthenticationHeaderValue authenticationHeaderValue = await this.tokenHelper.GetAuthenticationHeaderValueForSelectedAccount(selectedAccountCreds).ConfigureAwait(false);
            Tuple<AuthenticationHeaderValue, AccountCredential> selectedAccount = new Tuple<AuthenticationHeaderValue, AccountCredential>(authenticationHeaderValue, selectedAccountCreds);
            this.logger.TraceVerbose($"applicationFromAddress: {applicationFromAddress}", traceProps);

            // Override the From address of notifications based on application configuration
            notificationEntities.ToList().ForEach(nie => nie.From = applicationFromAddress);

            if (authenticationHeaderValue == null)
            {
                foreach (var item in notificationEntities)
                {
                    item.Status = NotificationItemStatus.Failed;
                    item.ErrorMessage = $"Could not retrieve authentication token with selected account:{selectedAccount.Item2?.AccountName} for the application:{applicationName}.";
                }
            }
            else
            {
                string emailAccountUsed = selectedAccount.Item2.AccountName;

                if (this.mSGraphSetting.EnableBatching)
                {
                    await this.ProcessEntitiesInBatch(applicationName, notificationEntities, selectedAccount).ConfigureAwait(false);
                }
                else
                {
                    await this.ProcessEntitiesIndividually(applicationName, notificationEntities, selectedAccount).ConfigureAwait(false);
                }
            }

            this.logger.TraceInformation($"Finished {nameof(this.ProcessNotificationEntities)} method of {nameof(MSGraphNotificationProvider)}.");
        }

        /// <summary>
        /// ProcessMeetingNotificationEntities.
        /// </summary>
        /// <param name="applicationName">applicationName.</param>
        /// <param name="notificationEntities">notificationEntities.</param>
        /// <returns>A <see cref="Task"/>.</returns>
        public Task ProcessMeetingNotificationEntities(string applicationName, IList<MeetingNotificationItemEntity> notificationEntities) => throw new NotImplementedException();

        /// <summary>
        /// Processes the notification items as individual tasks.
        /// </summary>
        /// <param name="applicationName">The application Name.</param>
        /// <param name="notificationEntities">List of notification entities to process.</param>
        /// <param name="emailAccountUsed">Email account used to process the notifications.</param>
        private async Task ProcessEntitiesIndividually(string applicationName, IList<EmailNotificationItemEntity> notificationEntities, Tuple<AuthenticationHeaderValue, AccountCredential> emailAccountUsed)
        {
            this.logger.TraceInformation($"Started {nameof(this.ProcessEntitiesIndividually)} method of {nameof(MSGraphNotificationProvider)}.");
            var traceProps = new Dictionary<string, string>();
            traceProps[Constants.Application] = applicationName;
            List<Task> emailNotificationSendTasks = new List<Task>();
            foreach (var item in notificationEntities)
            {
                item.EmailAccountUsed = emailAccountUsed.Item2.AccountName;
                item.TryCount++;
                item.ErrorMessage = string.Empty; // Reset the error message on next retry.
                try
                {
                    var sendForReal = this.mailSettings.Find(a => a.ApplicationName == applicationName).SendForReal;
                    var toOverride = this.mailSettings.Find(a => a.ApplicationName == applicationName).ToOverride;
                    var saveToSent = this.mailSettings.Find(a => a.ApplicationName == applicationName).SaveToSent;
                    MessageBody body = await this.emailManager.GetNotificationMessageBodyAsync(applicationName, item).ConfigureAwait(false);
                    EmailMessage message = item.ToGraphEmailMessage(body);
                    if (!sendForReal)
                    {
                        this.logger.TraceInformation($"Overriding the ToRecipients in {nameof(this.ProcessEntitiesIndividually)} method of {nameof(EmailManager)}.");
                        message.ToRecipients = toOverride.Split(Common.Constants.SplitCharacter, System.StringSplitOptions.RemoveEmptyEntries)
        .Select(torecipient => new Recipient { EmailAddress = new EmailAddress { Address = torecipient } }).ToList();
                        message.CCRecipients = null;
                        message.BCCRecipients = null;
                        message.ReplyToRecipients = null;
                    }

                    EmailMessagePayload payLoad = new EmailMessagePayload(message) { SaveToSentItems = saveToSent };
                    var isSuccess = await this.msGraphProvider.SendEmailNotification(emailAccountUsed.Item1, payLoad, item.NotificationId).ConfigureAwait(false);

                    item.Status = isSuccess ? NotificationItemStatus.Sent : (item.TryCount <= this.maxTryCount ? NotificationItemStatus.Retrying : NotificationItemStatus.Failed);
                }
                catch (AggregateException ex)
                {
                    item.Status = NotificationItemStatus.Failed;
                    item.ErrorMessage = (ex.InnerException != null) ? ex.InnerException.Message : ex.Message;
                }
            }

            this.logger.TraceInformation($"Finished {nameof(this.ProcessEntitiesIndividually)} method of {nameof(MSGraphNotificationProvider)}.");
        }

        /// <summary>
        /// Processes the notification items as a single batch to Graph.
        /// </summary>
        /// <param name="applicationName">The application Name.</param>
        /// <param name="notificationEntities">List of notification entities to process.</param>
        /// <param name="selectedAccount">selectedAccount.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        private async Task ProcessEntitiesInBatch(string applicationName, IList<EmailNotificationItemEntity> notificationEntities, Tuple<AuthenticationHeaderValue, AccountCredential> selectedAccount)
        {
            var traceProps = new Dictionary<string, string>();
            traceProps[Constants.Application] = applicationName;
            traceProps["EmailAccountUsed"] = selectedAccount.Item2.AccountName.Base64Encode();
            traceProps[Constants.EmailNotificationCount] = notificationEntities.Count.ToString(CultureInfo.InvariantCulture);

            this.logger.TraceInformation($"Started {nameof(this.ProcessEntitiesInBatch)} method of {nameof(MSGraphNotificationProvider)}.", traceProps);
            if (notificationEntities is null || notificationEntities.Count == 0)
            {
                throw new ArgumentNullException(nameof(notificationEntities));
            }

            var sendForReal = this.mailSettings.Find(a => a.ApplicationName == applicationName).SendForReal;
            var toOverride = this.mailSettings.Find(a => a.ApplicationName == applicationName).ToOverride;
            var saveToSent = this.mailSettings.Find(a => a.ApplicationName == applicationName).SaveToSent;

            // Step 1: Prepare graph requests from input entities that shall be sent in a batch to Graph.
            List<GraphBatchRequest> batchRequests = new List<GraphBatchRequest>();
            List<GraphRequest> graphRequests = new List<GraphRequest>();
            List<NotificationBatchItemResponse> batchItemResponses = new List<NotificationBatchItemResponse>();
            var nieItems = notificationEntities.ToList();
            foreach (var nie in nieItems)
            {
                EmailNotificationItemEntity item = nie;
                try
                {
                    MessageBody body = await this.emailManager.GetNotificationMessageBodyAsync(applicationName, item).ConfigureAwait(false);
                    EmailMessage message = nie.ToGraphEmailMessage(body);

                    if (!sendForReal)
                    {
                        this.logger.TraceInformation($"Overriding the ToRecipients in {nameof(this.ProcessEntitiesInBatch)} method of {nameof(EmailManager)}.", traceProps);
                        message.ToRecipients = toOverride.Split(Common.Constants.SplitCharacter, System.StringSplitOptions.RemoveEmptyEntries)
                        .Select(torecipient => new Recipient { EmailAddress = new EmailAddress { Address = torecipient } }).ToList();
                        message.CCRecipients = null;
                        message.BCCRecipients = null;
                        message.ReplyToRecipients = null;
                    }

                    graphRequests.Add(new GraphRequest()
                    {
                        Id = nie.NotificationId,
                        Url = this.mSGraphSetting.SendMailUrl.StartsWith("/", StringComparison.OrdinalIgnoreCase) ? this.mSGraphSetting.SendMailUrl : $"/{this.mSGraphSetting.SendMailUrl}",
                        Body = new EmailMessagePayload(message) { SaveToSentItems = saveToSent },
                        Headers = new GraphRequestHeaders() { ContentType = Constants.JsonMIMEType },
                        Method = Constants.POSTHttpVerb,
                    });
                }
#pragma warning disable CA1031 // Do not catch general exception types
                catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
                {
                    this.logger.TraceInformation($"Caught exception while creating the graph Message {nameof(this.ProcessEntitiesInBatch)} method of {nameof(MSGraphNotificationProvider)}.", traceProps);
                    batchItemResponses.Add(new NotificationBatchItemResponse { Error = (ex.InnerException != null) ? ex.InnerException.Message : ex.Message, NotificationId = item.NotificationId, Status = HttpStatusCode.PreconditionFailed });
                }
            }

            // Step 2: Split the full list of requests into smaller chunks as per the Graph Batch request limit.
            List<List<GraphRequest>> splitGraphRequests = BusinessUtilities.SplitList(graphRequests, this.mSGraphSetting.BatchRequestLimit).ToList();
            foreach (var graphRequestChunk in splitGraphRequests)
            {
                batchRequests.Add(new GraphBatchRequest() { Requests = graphRequestChunk });
            }

            // Step 3: Invoke the Graph API for each batch request chunk prepared above.
            foreach (var batchRequest in batchRequests)
            {
                batchItemResponses.AddRange(await this.msGraphProvider.ProcessEmailRequestBatch(selectedAccount.Item1, batchRequest).ConfigureAwait(false));
            }

            bool isAccountIndexIncremented = false;

            // Step 4: Loop through the responses and set the status of the input entities.
            foreach (var item in notificationEntities)
            {
                item.EmailAccountUsed = selectedAccount.Item2.AccountName;
                item.TryCount++;
                item.ErrorMessage = string.Empty; // Reset the error message on next retry.
                var itemResponse = batchItemResponses.Find(resp => resp.NotificationId == item.NotificationId);
                if (itemResponse?.Status == HttpStatusCode.Accepted)
                {
                    item.Status = NotificationItemStatus.Sent;
                }
                else if (item.TryCount <= this.maxTryCount && (itemResponse?.Status == HttpStatusCode.TooManyRequests || itemResponse?.Status == HttpStatusCode.RequestTimeout))
                {
                    // Mark these items as queued and Queue them
                    item.Status = NotificationItemStatus.Retrying;
                    item.ErrorMessage = itemResponse?.Error;
                    if (itemResponse.Error?.Contains("quota was exceeded", StringComparison.InvariantCultureIgnoreCase) ?? false)
                    {
                        this.logger.WriteCustomEvent($"Mail Box Exhausted :  {item.EmailAccountUsed} ");
                        this.logger.TraceInformation($"{itemResponse.Error} Item with notification id={item.NotificationId} will be retried with a different mail box");
                        if (!isAccountIndexIncremented)
                        {
                            isAccountIndexIncremented = true;
                            this.emailAccountManager.IncrementIndex();
                        }
                    }
                }
                else
                {
                    item.Status = NotificationItemStatus.Failed;
                    item.ErrorMessage = itemResponse?.Error;
                }
            }

            this.logger.TraceInformation($"Finished {nameof(this.ProcessEntitiesInBatch)} method of {nameof(MSGraphNotificationProvider)}.");
        }
    }
}
