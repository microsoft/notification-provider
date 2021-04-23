// Copyright (c) Microsoft Corporation.
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
    using NotificationService.BusinessLibrary.Models;
    using NotificationService.Common;
    using NotificationService.Common.Configurations;
    using NotificationService.Common.Logger;
    using NotificationService.Common.Utility;
    using NotificationService.Contracts;
    using NotificationService.Contracts.Entities;
    using NotificationService.Contracts.Models.Graph;
    using NotificationService.Contracts.Models.Graph.Invite;

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
        /// <param name="tokenHelper">Instance of <see cref="ITokenHelper"/>.</param>
        /// <param name="msGraphProvider">Instance of <see cref="IMSGraphProvider"/>.</param>
        /// <param name="emailManager">Instance of <see cref="IEmailManager"/>..</param>
        public MSGraphNotificationProvider(
             IConfiguration configuration,
             IEmailAccountManager emailAccountManager,
             ILogger logger,
             IOptions<MSGraphSetting> mSGraphSetting,
             ITokenHelper tokenHelper,
             IMSGraphProvider msGraphProvider,
             IEmailManager emailManager)
        {
            this.configuration = configuration;
            this.applicationAccounts = JsonConvert.DeserializeObject<List<ApplicationAccounts>>(this.configuration?[ConfigConstants.ApplicationAccountsConfigSectionKey]);
            this.emailAccountManager = emailAccountManager;
            this.logger = logger;
            _ = int.TryParse(this.configuration[$"{ConfigConstants.RetrySettingConfigSectionKey}:{ConfigConstants.RetrySettingMaxRetryCountConfigKey}"], out this.maxTryCount);
            if (this.configuration?[ConfigConstants.MailSettingsConfigKey] != null)
            {
                this.mailSettings = JsonConvert.DeserializeObject<List<MailSettings>>(this.configuration?[ConfigConstants.MailSettingsConfigKey]);
            }

            this.mSGraphSetting = mSGraphSetting?.Value;
            this.tokenHelper = tokenHelper;
            this.msGraphProvider = msGraphProvider;
            this.emailManager = emailManager;
        }

        /// <inheritdoc/>
        public async Task ProcessNotificationEntities(string applicationName, IList<EmailNotificationItemEntity> notificationEntities)
        {
            if (notificationEntities is null || notificationEntities.Count == 0)
            {
                throw new ArgumentNullException(nameof(notificationEntities), "notificationEntities are null.");
            }

            var traceProps = new Dictionary<string, string>();
            traceProps[AIConstants.Application] = applicationName;
            traceProps[AIConstants.EmailNotificationCount] = notificationEntities.Count.ToString(CultureInfo.InvariantCulture);

            this.logger.TraceInformation($"Started {nameof(this.ProcessNotificationEntities)} method of {nameof(MSGraphNotificationProvider)}.", traceProps);
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

            this.logger.TraceInformation($"Finished {nameof(this.ProcessNotificationEntities)} method of {nameof(MSGraphNotificationProvider)}.", traceProps);
        }

        /// <inheritdoc/>
        public async Task ProcessMeetingNotificationEntities(string applicationName, IList<MeetingNotificationItemEntity> meetingInviteEntities)
        {
            var traceProps = new Dictionary<string, string>();
            traceProps[AIConstants.Application] = applicationName;
            this.logger.TraceInformation($"Started {nameof(this.ProcessMeetingNotificationEntities)} method of {nameof(MSGraphNotificationProvider)}.", traceProps);
            if (meetingInviteEntities is null || meetingInviteEntities.Count == 0)
            {
                throw new ArgumentNullException(nameof(meetingInviteEntities), "meetingInviteEntities are null/empty.");
            }

            traceProps[AIConstants.EmailNotificationCount] = meetingInviteEntities?.Count.ToString(CultureInfo.InvariantCulture);
            var applicationFromAddress = this.applicationAccounts.Find(a => a.ApplicationName == applicationName).FromOverride;
            AccountCredential selectedAccountCreds = this.emailAccountManager.FetchAccountToBeUsedForApplication(applicationName, this.applicationAccounts);
            AuthenticationHeaderValue authenticationHeaderValue = await this.tokenHelper.GetAuthenticationHeaderValueForSelectedAccount(selectedAccountCreds).ConfigureAwait(false);
            Tuple<AuthenticationHeaderValue, AccountCredential> selectedAccount = new Tuple<AuthenticationHeaderValue, AccountCredential>(authenticationHeaderValue, selectedAccountCreds);
            this.logger.TraceVerbose($"applicationFromAddress: {applicationFromAddress}", traceProps);
            meetingInviteEntities.ToList().ForEach(nie => nie.From = applicationFromAddress);

            if (authenticationHeaderValue == null)
            {
                foreach (var item in meetingInviteEntities)
                {
                    item.Status = NotificationItemStatus.Failed;
                    item.ErrorMessage = $"Could not retrieve authentication token with selected account:{selectedAccount.Item2?.AccountName} for the application:{applicationName}.";
                }
            }
            else
            {
                string emailAccountUsed = selectedAccount.Item2.AccountName;
                await this.ProcessMeetingEntitiesIndividually(applicationName, meetingInviteEntities, selectedAccount).ConfigureAwait(false);
            }

            this.logger.TraceInformation($"Finished {nameof(this.ProcessMeetingNotificationEntities)} method of {nameof(MSGraphNotificationProvider)}.", traceProps);
        }

        /// <summary>
        /// Validates for all attachments sent.
        /// </summary>
        /// <param name="res">HttpResponse object after send attachment request using httpclient.</param>
        /// <param name="item">Meeting Notification Item object.</param>
        /// <returns>a boolean value for success/failure.</returns>
        private static bool IsAllAttachmentsSent(IDictionary<string, ResponseData<string>> res, MeetingNotificationItemEntity item)
        {
            if (res == null)
            {
                return false;
            }

            var successResponse = res.Values.Where(a => a.Status == true);
            return successResponse.Count() == item.Attachments.Count();
        }

        /// <summary>
        /// Processes the notification items as individual tasks.
        /// </summary>
        /// <param name="applicationName">The application Name.</param>
        /// <param name="notificationEntities">List of notification entities to process.</param>
        /// <param name="emailAccountUsed">Email account used to process the notifications.</param>
        private async Task ProcessEntitiesIndividually(string applicationName, IList<EmailNotificationItemEntity> notificationEntities, Tuple<AuthenticationHeaderValue, AccountCredential> emailAccountUsed)
        {
            var traceProps = new Dictionary<string, string>();
            traceProps[AIConstants.Application] = applicationName;
            this.logger.TraceInformation($"Started {nameof(this.ProcessEntitiesIndividually)} method of {nameof(MSGraphNotificationProvider)}.", traceProps);

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
                        this.logger.TraceInformation($"Overriding the ToRecipients in {nameof(this.ProcessEntitiesIndividually)} method of {nameof(EmailManager)}.", traceProps);
                        message.ToRecipients = toOverride.Split(Common.ApplicationConstants.SplitCharacter, System.StringSplitOptions.RemoveEmptyEntries).Select(torecipient => new Recipient { EmailAddress = new EmailAddress { Address = torecipient } }).ToList();
                        message.CCRecipients = null;
                        message.BCCRecipients = null;
                        message.ReplyToRecipients = null;
                    }

                    EmailMessagePayload payLoad = new EmailMessagePayload(message) { SaveToSentItems = saveToSent };
                    var response = await this.msGraphProvider.SendEmailNotification(emailAccountUsed.Item1, payLoad, item.NotificationId).ConfigureAwait(false);
                    if (response.Status)
                    {
                        item.Status = NotificationItemStatus.Sent;
                    }
                    else if (item.TryCount <= this.maxTryCount && (response.StatusCode == HttpStatusCode.TooManyRequests || response.StatusCode == HttpStatusCode.RequestTimeout))
                    {
                        item.Status = NotificationItemStatus.Retrying;
                        item.ErrorMessage = response.Result;
                        _ = this.IsMailboxLimitExchausted(response.Result, item.NotificationId, item.EmailAccountUsed, false, traceProps);
                    }
                    else
                    {
                        this.logger.WriteCustomEvent($"{AIConstants.CustomEventMailSendFailed} for notificationId:  {item.NotificationId} ");
                        item.Status = NotificationItemStatus.Failed;
                        item.ErrorMessage = response.Result;
                    }
                }
                catch (AggregateException ex)
                {
                    this.logger.WriteCustomEvent($"{AIConstants.CustomEventMailSendFailed} for notificationId:  {item.NotificationId}");
                    item.Status = NotificationItemStatus.Failed;
                    item.ErrorMessage = (ex.InnerException != null) ? ex.InnerException.Message : ex.Message;
                }
            }

            this.logger.TraceInformation($"Finished {nameof(this.ProcessEntitiesIndividually)} method of {nameof(MSGraphNotificationProvider)}.", traceProps);
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
            traceProps[AIConstants.Application] = applicationName;
            traceProps["EmailAccountUsed"] = selectedAccount.Item2.AccountName.Base64Encode();
            traceProps[AIConstants.EmailNotificationCount] = notificationEntities.Count.ToString(CultureInfo.InvariantCulture);

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
                        message.ToRecipients = toOverride.Split(Common.ApplicationConstants.SplitCharacter, System.StringSplitOptions.RemoveEmptyEntries)
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
                        Headers = new GraphRequestHeaders() { ContentType = ApplicationConstants.JsonMIMEType },
                        Method = ApplicationConstants.POSTHttpVerb,
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
                    isAccountIndexIncremented = this.IsMailboxLimitExchausted(itemResponse?.Error, item.NotificationId, item.EmailAccountUsed, isAccountIndexIncremented, traceProps);
                }
                else
                {
                    this.logger.WriteCustomEvent($"{AIConstants.CustomEventMailSendFailed} for notificationId:  {item.NotificationId} ");
                    item.Status = NotificationItemStatus.Failed;
                    item.ErrorMessage = itemResponse?.Error;
                }
            }

            this.logger.TraceInformation($"Finished {nameof(this.ProcessEntitiesInBatch)} method of {nameof(MSGraphNotificationProvider)}.");
        }

        /// <summary>
        /// Process meeting invites individually.
        /// </summary>
        /// <param name="applicationName">the application Name.</param>
        /// <param name="notificationEntities">List of Meeting Notification Entities to be sent.</param>
        /// <param name="selectedAccount">email account used for sending meeting invites.</param>
        /// <returns>A<see cref="Task"/> representing the result of the asynchronous operation.</returns>
        private async Task ProcessMeetingEntitiesIndividually(string applicationName, IList<MeetingNotificationItemEntity> notificationEntities, Tuple<AuthenticationHeaderValue, AccountCredential> selectedAccount)
        {
            var traceProps = new Dictionary<string, string>();
            traceProps[AIConstants.Application] = applicationName;
            traceProps[AIConstants.NotificationType] = NotificationType.Meet.ToString();
            this.logger.TraceInformation($"Started {nameof(this.ProcessMeetingEntitiesIndividually)} method of {nameof(MSGraphNotificationProvider)}.", traceProps);
            if (notificationEntities is null || notificationEntities.Count == 0)
            {
                throw new ArgumentNullException(nameof(notificationEntities), "notificationEntities are null.");
            }

            if (selectedAccount == null)
            {
                throw new ArgumentNullException(nameof(selectedAccount), "selectedAccount for sending meeting invite is null");
            }

            var sendForReal = this.mailSettings.Find(a => a.ApplicationName == applicationName).SendForReal;
            var toOverride = this.mailSettings.Find(a => a.ApplicationName == applicationName).ToOverride;

            foreach (var item in notificationEntities)
            {
                item.EmailAccountUsed = selectedAccount.Item2.AccountName;
                item.TryCount++;
                item.ErrorMessage = string.Empty;
                try
                {
                    var payload = await this.CreateInvitePayload(item, applicationName).ConfigureAwait(false);
                    if (!sendForReal)
                    {
                        this.logger.TraceInformation($"Overriding the ToRecipients in {nameof(this.ProcessMeetingEntitiesIndividually)} method of {nameof(MSGraphNotificationProvider)}. notificationId {item.NotificationId}", traceProps);
                        payload.Attendees = toOverride.Split(Common.ApplicationConstants.SplitCharacter, System.StringSplitOptions.RemoveEmptyEntries).Select(torecipient => new Attendee { EmailAddress = new EmailAddress { Address = torecipient } }).ToList();
                        payload.Organizer = null;
                    }

                    var result = await this.msGraphProvider.SendMeetingInvite(selectedAccount.Item1, payload, item.NotificationId).ConfigureAwait(false);
                    if (result.Status)
                    {
                        item.Status = NotificationItemStatus.Sent;
                        var responseObj = !string.IsNullOrEmpty(result.Result) ? JsonConvert.DeserializeObject<InviteResponse>(result.Result) : null;
                        if (responseObj != null)
                        {
                            item.EventId = responseObj.EventId;
                        }

                        if (payload.HasAttachments)
                        {
                            var attachments = item.Attachments.Select(a => new FileAttachment()
                            {
                                ContentBytes = a.FileBase64,
                                Name = a.FileName,
                                IsInline = a.IsInline,
                            }).ToList();

                            var res = this.msGraphProvider.SendMeetingInviteAttachments(selectedAccount.Item1, attachments, item.EventId, item.NotificationId);
                            if (!IsAllAttachmentsSent(res, item))
                            {
                                // If all attachments sent is failed. delete the event and mark it for retrial in next run.
                                _ = this.msGraphProvider.DeleteMeetingInvite(selectedAccount.Item1, item.NotificationId, item.EventId);
                                item.Status = item.TryCount <= this.maxTryCount ? NotificationItemStatus.Retrying : NotificationItemStatus.Failed;
                                item.ErrorMessage = "All attachments were not send successfully.";
                            }
                        }
                    }
                    else
                    {
                        if (item.TryCount <= this.maxTryCount && (result.StatusCode == HttpStatusCode.TooManyRequests || result.StatusCode == HttpStatusCode.RequestTimeout))
                        {
                            item.Status = NotificationItemStatus.Retrying;
                            _ = this.IsMailboxLimitExchausted(result.Result, item.NotificationId, item.EmailAccountUsed, false, traceProps);
                        }
                        else
                        {
                            this.logger.WriteCustomEvent($"{AIConstants.CustomEventInviteSendFailed} for notificationId:  {item.NotificationId} ");
                            item.Status = NotificationItemStatus.Failed;
                        }

                        item.ErrorMessage = result.Result;
                        this.logger.TraceInformation($"{nameof(this.ProcessMeetingEntitiesIndividually)} of class {nameof(MSGraphNotificationProvider)} : Putting the invite back for next retry. Current request statusCode: {result.StatusCode} for notificationId {item.NotificationId}", traceProps);
                    }
                }
                catch (AggregateException ex)
                {
                    this.logger.WriteCustomEvent($"{AIConstants.CustomEventInviteSendFailed} for notificationId:  {item.NotificationId}");
                    item.Status = NotificationItemStatus.Failed;
                    item.ErrorMessage = (ex.InnerException != null) ? ex.InnerException.Message : ex.Message;
                }
            }

            this.logger.TraceInformation($"Finished {nameof(this.ProcessMeetingEntitiesIndividually)} method of {nameof(MSGraphNotificationProvider)}.", traceProps);
        }

        /// <summary>
        /// create Payload for Meeting Invite to be set to Graph API.
        /// </summary>
        /// <param name="meetingNotificationEntity"> MeetingNotification Entity Object.</param>
        /// <param name="applicationName">Application Name for the Meeting Invite.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        private async Task<InvitePayload> CreateInvitePayload(MeetingNotificationItemEntity meetingNotificationEntity, string applicationName)
        {
            if (meetingNotificationEntity == null)
            {
                return null;
            }

            var payload = new InvitePayload();

            var requiredAttendees = meetingNotificationEntity.RequiredAttendees.Split(Common.ApplicationConstants.SplitCharacter, System.StringSplitOptions.RemoveEmptyEntries).Select(e => new Attendee()
            {
                EmailAddress = new EmailAddress()
                {
                    Address = e,
                },
                Type = AttendeeType.Required,
            });
            var optionalAttendees = meetingNotificationEntity.OptionalAttendees?.Split(Common.ApplicationConstants.SplitCharacter, System.StringSplitOptions.RemoveEmptyEntries).Select(e => new Attendee()
            {
                EmailAddress = new EmailAddress()
                {
                    Address = e,
                },
                Type = AttendeeType.Optional,
            });

            payload.Attendees = (optionalAttendees != null ? requiredAttendees.Union(optionalAttendees) : requiredAttendees).ToList();
            payload.Body = await this.emailManager.GetMeetingInviteBodyAsync(applicationName, meetingNotificationEntity).ConfigureAwait(false);
            payload.End = new InviteDateTime()
            {
                DateTime = meetingNotificationEntity.End.FormatDate(ApplicationConstants.GraphMeetingInviteDateTimeFormatter),
            };
            payload.Importance = (ImportanceType)Enum.Parse(typeof(ImportanceType), meetingNotificationEntity.Priority.ToString());
            payload.IsCancelled = meetingNotificationEntity.IsCancel;
            payload.IsOnlineMeeting = meetingNotificationEntity.IsOnlineMeeting;
            payload.IsAllDay = meetingNotificationEntity.IsAllDayEvent;
            payload.Location = new Location()
            {
                DisplayName = meetingNotificationEntity.Location,
            };
            payload.Organizer = new Organizer()
            {
                EmailAddress = new EmailAddress()
                {
                    Address = meetingNotificationEntity.From,
                },
            };

            if (meetingNotificationEntity.RecurrencePattern != MeetingRecurrencePattern.None)
            {
                var recurrencePattern = new RecurrencePattern()
                {
                    Type = (RecurrencePatternType)Enum.Parse(typeof(RecurrencePatternType), meetingNotificationEntity.RecurrencePattern.ToString()),
                    Interval = meetingNotificationEntity.Interval,
                    DaysOfWeek = meetingNotificationEntity.DaysOfWeek.GetListFromString<Contracts.Models.Graph.Invite.DayOfTheWeek>(','),
                    DayOfMonth = meetingNotificationEntity.DayofMonth,
                    Month = meetingNotificationEntity.MonthOfYear,
                };
                var recurrenceRangeType = meetingNotificationEntity.EndDate.HasValue ? RecurrenceRangeType.EndDate : meetingNotificationEntity.Ocurrences.HasValue ? RecurrenceRangeType.Numbered : RecurrenceRangeType.NoEnd;
                var recurrenceRange = new RecurrenceRange()
                {
                    NumberOfOccurences = meetingNotificationEntity.Ocurrences,
                    EndDate = meetingNotificationEntity.EndDate.HasValue ? meetingNotificationEntity.EndDate?.FormatDate(ApplicationConstants.GraphMeetingInviteRecurrenceRangeDateFormatter) : null,
                    StartDate = meetingNotificationEntity.Start.FormatDate(ApplicationConstants.GraphMeetingInviteRecurrenceRangeDateFormatter),
                    Type = recurrenceRangeType,
                };
                payload.Recurrence = new Recurrence()
                {
                    Pattern = recurrencePattern,
                    Range = recurrenceRange,
                };
            }

            payload.ReminderMinutesBeforeStart = Convert.ToInt32(meetingNotificationEntity.ReminderMinutesBeforeStart, CultureInfo.InvariantCulture);
            payload.Start = new InviteDateTime()
            {
                DateTime = meetingNotificationEntity.Start.FormatDate(ApplicationConstants.GraphMeetingInviteDateTimeFormatter),
            };

            payload.Subject = meetingNotificationEntity.Subject;
            payload.TransactionId = meetingNotificationEntity.NotificationId;
            payload.HasAttachments = meetingNotificationEntity.Attachments != null && meetingNotificationEntity.Attachments.Any() ? true : false;
            payload.ICallUid = meetingNotificationEntity.ICalUid;
            return payload;
        }

        /// <summary>
        /// Logs Event and telemtry for exhausted mailbox.
        /// </summary>
        /// <param name="errorMessage">errormessage from http api call.</param>
        /// <param name="notificationId">unique identifier or notification.</param>
        /// <param name="mailboxUsed">mailbox used to send notificaiton.</param>
        /// <param name="isAccountIndexIncremented">to track the index of already exchausted mailbox.</param>
        /// <param name="traceProps">telemetry properties to be logged.</param>
        /// <returns>return the update status of already incremented index.</returns>
        private bool IsMailboxLimitExchausted(string errorMessage, string notificationId, string mailboxUsed, bool isAccountIndexIncremented = false, IDictionary<string, string> traceProps = null)
        {
            if (errorMessage?.Contains("quota was exceeded", StringComparison.InvariantCultureIgnoreCase) ?? false)
            {
                this.logger.WriteCustomEvent($"{AIConstants.CustomEventMailBoxExhausted}  for mailbox account :  {mailboxUsed} ");
                this.logger.TraceInformation($"{errorMessage} Item with notification id={notificationId} will be retried with a different mail box", traceProps);
                if (!isAccountIndexIncremented)
                {
                    isAccountIndexIncremented = true;
                    this.emailAccountManager.IncrementIndex();
                }
            }

            return isAccountIndexIncremented;
        }
    }
}
