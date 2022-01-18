// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.BusinessLibrary
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using NotificationService.BusinessLibrary.Models;
    using NotificationService.Common;
    using NotificationService.Common.Logger;
    using NotificationService.Contracts;
    using NotificationService.Contracts.Models.Graph.Invite;

    /// <summary>
    /// Provider to interact with MS Graph APIs.
    /// </summary>
    public class MSGraphProvider : IMSGraphProvider
    {
        /// <summary>
        /// Http Client for Graph Provider.
        /// </summary>
        private readonly HttpClient httpClient;

        /// <summary>
        /// JSON serializer settings for http calls.
        /// </summary>
        private readonly JsonSerializerSettings jsonSerializerSettings;

        /// <summary>
        /// MS Graph configuration.
        /// </summary>
        private readonly MSGraphSetting mSGraphSetting;

        /// <summary>
        /// Polly Retry Setting.
        /// </summary>
        private readonly RetrySetting pollyRetrySetting;

        /// <summary>
        /// Instance of <see cref="ILogger"/>.
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="MSGraphProvider"/> class.
        /// </summary>
        /// <param name="mSGraphSetting">MS Graph Settings from configuration.</param>
        /// <param name="pollyRetrySetting">Polly Retry Settings.</param>
        /// <param name="logger">Instance of Logger.</param>
        /// <param name="httpClient">Http Client.</param>
        public MSGraphProvider(IOptions<MSGraphSetting> mSGraphSetting, IOptions<RetrySetting> pollyRetrySetting, ILogger logger, HttpClient httpClient)
        {
            this.jsonSerializerSettings = new JsonSerializerSettings
            {
                Converters = new List<JsonConverter> { new StringEnumConverter() },
                ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver(),
                NullValueHandling = NullValueHandling.Ignore,
            };
            this.mSGraphSetting = mSGraphSetting?.Value;
            this.pollyRetrySetting = pollyRetrySetting?.Value;
            this.logger = logger;
            this.httpClient = httpClient;
        }

        /// <inheritdoc/>
        public async Task<IList<NotificationBatchItemResponse>> ProcessEmailRequestBatch(AuthenticationHeaderValue authenticationHeaderValue, GraphBatchRequest graphBatchRequest)
        {
            this.logger.TraceInformation($"Started {nameof(this.ProcessEmailRequestBatch)} method of {nameof(MSGraphProvider)}.");
            List<NotificationBatchItemResponse> responses = new List<NotificationBatchItemResponse>();

            this.httpClient.DefaultRequestHeaders.Authorization = authenticationHeaderValue;
            var requestPayLoad = JsonConvert.SerializeObject(graphBatchRequest, this.jsonSerializerSettings);
            HttpResponseMessage response = null;
            response = await this.httpClient.PostAsync(
            $"{this.mSGraphSetting.BaseUrl}/{this.mSGraphSetting.GraphAPIVersion}/{this.mSGraphSetting.BatchRequestUrl}",
            new StringContent(requestPayLoad, Encoding.UTF8, ApplicationConstants.JsonMIMEType)).ConfigureAwait(false);

            this.logger.TraceInformation($"Method {nameof(this.ProcessEmailRequestBatch)}:Completed Graph Batch Call");
            var traceProps = new Dictionary<string, string>();
            traceProps["GraphRequest"] = requestPayLoad;
            traceProps["GraphResponse"] = JsonConvert.SerializeObject(response, this.jsonSerializerSettings);
            this.logger.TraceInformation($"Graph Request and Graph Response", traceProps);

            _ = traceProps.Remove("GraphRequest");
            _ = traceProps.Remove("GraphResponse");

            if (response?.IsSuccessStatusCode ?? false)
            {
                // Read and deserialize response.
                var graphResponses = JsonConvert.DeserializeObject<GraphBatchResponse>(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
                foreach (var graphResponse in graphResponses?.Responses)
                {
                    if (graphResponse?.Body?.Error?.Code != null)
                    {
                        traceProps["NotificationId"] = graphResponse?.Id;
                        traceProps["ErrorCode"] = graphResponse?.Body?.Error?.Code;
                        this.logger.WriteCustomEvent("Error Sending Notification", traceProps);
                    }

                    responses.Add(graphResponse.ToNotificationBatchItemResponse());
                }
            }
            else
            {
                string content = string.Empty;
                if (response != null)
                {
                    content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                }

                throw new System.Exception($"An error occurred while processing notification batch. Details: {content}");
            }

            this.logger.TraceInformation($"Finished {nameof(this.ProcessEmailRequestBatch)} method of {nameof(MSGraphProvider)}.");
            return responses;
        }

        /// <inheritdoc/>
        public async Task<ResponseData<string>> SendEmailNotification(AuthenticationHeaderValue authenticationHeaderValue, EmailMessagePayload payLoad, string notificationId)
        {
            this.logger.TraceInformation($"Started {nameof(this.SendEmailNotification)} method of {nameof(MSGraphProvider)}.");
            this.httpClient.DefaultRequestHeaders.Authorization = authenticationHeaderValue;
            var requestPayLoad = JsonConvert.SerializeObject(payLoad, this.jsonSerializerSettings);
            HttpResponseMessage response = null;
            response = await this.httpClient.PostAsync(
                    $"{this.mSGraphSetting.BaseUrl}/{this.mSGraphSetting.GraphAPIVersion}/{this.mSGraphSetting.SendMailUrl}",
                    new StringContent(requestPayLoad, Encoding.UTF8, ApplicationConstants.JsonMIMEType)).ConfigureAwait(false);

            this.logger.TraceInformation($"Method {nameof(this.SendEmailNotification)}: Completed Graph Send Email Call for notificationId : {notificationId}");

            var responseData = await GetResponseData(response).ConfigureAwait(false);

            if (responseData == null || (!responseData.Status && !(responseData.StatusCode == HttpStatusCode.TooManyRequests || responseData.StatusCode == HttpStatusCode.RequestTimeout)))
            {
                throw new System.Exception($"An error occurred while sending notification id: {notificationId}. Details: {responseData?.Result}");
            }

            this.logger.TraceInformation($"Finished {nameof(this.SendEmailNotification)} method of {nameof(MSGraphProvider)}.");
            return responseData;
        }

        /// <inheritdoc/>
        public async Task<ResponseData<string>> SendMeetingInvite(AuthenticationHeaderValue authenticationHeaderValue, InvitePayload payLoad, string notificationId)
        {
            this.logger.TraceInformation($"Started {nameof(this.SendMeetingInvite)} method of {nameof(MSGraphProvider)}.");
            this.httpClient.DefaultRequestHeaders.Authorization = authenticationHeaderValue;
            var requestPayLoad = JsonConvert.SerializeObject(payLoad, this.jsonSerializerSettings);
            HttpResponseMessage response = null;
            response = await this.httpClient.PostAsync(
                    $"{this.mSGraphSetting.BaseUrl}/{this.mSGraphSetting.GraphAPIVersion}/{this.mSGraphSetting.SendInviteUrl}",
                    new StringContent(requestPayLoad, Encoding.UTF8, ApplicationConstants.JsonMIMEType)).ConfigureAwait(false);

            this.logger.TraceInformation($"Method {nameof(this.SendMeetingInvite)}: Completed Graph Send Meeting Invite Call for notificationId {notificationId}");

            var responseData = await GetResponseData(response).ConfigureAwait(false);

            if (responseData == null || (!responseData.Status && !(responseData.StatusCode == HttpStatusCode.TooManyRequests || responseData.StatusCode == HttpStatusCode.RequestTimeout)))
            {
                throw new System.Exception($"An error occurred while sending notification id: {notificationId}. Details: {responseData?.Result}");
            }

            this.logger.TraceInformation($"Finished {nameof(this.SendMeetingInvite)} method of {nameof(MSGraphProvider)}.");
            return responseData;
        }

        /// <inheritdoc/>
        public async Task<ResponseData<string>> UpdateMeetingInvite(AuthenticationHeaderValue authenticationHeaderValue, InvitePayload payLoad, string notificationId, string eventId)
        {
            this.logger.TraceInformation($"Started {nameof(this.UpdateMeetingInvite)} method of {nameof(MSGraphProvider)}.");
            this.httpClient.DefaultRequestHeaders.Authorization = authenticationHeaderValue;
            var requestPayLoad = JsonConvert.SerializeObject(payLoad, this.jsonSerializerSettings);
            HttpResponseMessage response = null;
            response = await this.httpClient.PatchAsync(
                    $"{this.mSGraphSetting.BaseUrl}/{this.mSGraphSetting.GraphAPIVersion}/{this.mSGraphSetting.SendInviteUrl}/{eventId}",
                    new StringContent(requestPayLoad, Encoding.UTF8, ApplicationConstants.JsonMIMEType)).ConfigureAwait(false);
            this.logger.TraceInformation($"Method {nameof(this.UpdateMeetingInvite)}: Completed Graph Update Meeting Invite Call for notificationId {notificationId}");

            var responseData = await GetResponseData(response).ConfigureAwait(false);

            if (responseData == null || (!responseData.Status && !(responseData.StatusCode == HttpStatusCode.TooManyRequests || responseData.StatusCode == HttpStatusCode.RequestTimeout)))
            {
                throw new System.Exception($"An error occurred while sending notification id: {notificationId}. Details: {responseData?.Result}");
            }

            this.logger.TraceInformation($"Finished {nameof(this.UpdateMeetingInvite)} method of {nameof(MSGraphProvider)}.");
            return responseData;
        }

        /// <inheritdoc/>
        public async Task<ResponseData<string>> DeleteMeetingInvite(AuthenticationHeaderValue authenticationHeaderValue, string notificationId, string eventId)
        {
            this.logger.TraceInformation($"Started {nameof(this.DeleteMeetingInvite)} method of {nameof(MSGraphProvider)}.");
            this.httpClient.DefaultRequestHeaders.Authorization = authenticationHeaderValue;
            HttpResponseMessage response = null;
            response = await this.httpClient.DeleteAsync(
                    $"{this.mSGraphSetting.BaseUrl}/{this.mSGraphSetting.GraphAPIVersion}/{this.mSGraphSetting.SendInviteUrl}/{eventId}").ConfigureAwait(false);
            this.logger.TraceInformation($"Method {nameof(this.DeleteMeetingInvite)}: Completed Graph Delete Meeting Invite Call for NotificationId {notificationId}");

            var responseData = await GetResponseData(response).ConfigureAwait(false);

            if (responseData == null || (!responseData.Status && !(responseData.StatusCode == HttpStatusCode.TooManyRequests || responseData.StatusCode == HttpStatusCode.RequestTimeout)))
            {
                throw new System.Exception($"An error occurred while sending notification id: {notificationId}. Details: {responseData?.Result}");
            }

            this.logger.TraceInformation($"Finished {nameof(this.DeleteMeetingInvite)} method of {nameof(MSGraphProvider)}.");
            return responseData;
        }

        /// <inheritdoc/>
        public IDictionary<string, ResponseData<string>> SendMeetingInviteAttachments(AuthenticationHeaderValue authenticationHeaderValue, List<FileAttachment> attachments, string eventId, string notificationId)
        {
            var result = new Dictionary<string, ResponseData<string>>();
            if (attachments == null || attachments.Count == 0)
            {
                return result;
            }

            var maxRetryCount = this.pollyRetrySetting.MaxRetries;
            IList<Task> tasks = new List<Task>();
            foreach (var attachment in attachments)
            {
                tasks.Add(Task.Run(() =>
                {
                    int count = 0;
                    ResponseData<string> response = null;
                    do
                    {
                        try
                        {
                            count++;
                            response = this.SendMeetingInviteAttachment(attachment, authenticationHeaderValue, eventId, notificationId).GetAwaiter().GetResult();
                            if (result.ContainsKey(attachment.Name))
                            {
                                _ = result.Remove(attachment.Name);
                            }

                            result.Add(attachment.Name, response);
                        }
#pragma warning disable CA1031 // Do not catch general exception types
                        catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
                        {
                            this.logger.TraceError($"Error {nameof(this.SendMeetingInviteAttachments)} method: sending attachment [{attachment.Name}] and notificationId [{notificationId}] in trial [{count}] with exception {ex}");
                        }
                    }
                    while ((response == null || !response.Status) && count < maxRetryCount);
                }));
            }

            Task.WaitAll(tasks.ToArray());

            return result;
        }

        /// <summary>
        /// Gets ResponseData from httpResponse.
        /// </summary>
        /// <param name="response"> HttpResponse Object. </param>
        /// <returns> ResponseData.</returns>
        private static async Task<ResponseData<string>> GetResponseData(HttpResponseMessage response)
        {
            if (response == null)
            {
                return null;
            }

            var result = new ResponseData<string>();
            result.Status = response.IsSuccessStatusCode;
            result.StatusCode = response.StatusCode;
            result.Result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return result;
        }

        /// <summary>
        /// Sends attachment to the event alread sent.
        /// </summary>
        /// <param name="attachment">List of Attachment object to be sent. </param>
        /// <param name="authenticationHeaderValue">Authentication header corresponding to the sender of the email.</param>
        /// <param name="eventId">EventId as reference to already sent event.</param>
        /// <param name="notificationId"> Internal identifier of the email message to be sent. </param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        private async Task<ResponseData<string>> SendMeetingInviteAttachment(FileAttachment attachment, AuthenticationHeaderValue authenticationHeaderValue, string eventId, string notificationId)
        {
            this.logger.TraceInformation($"Started {nameof(this.SendMeetingInviteAttachment)} method of {nameof(MSGraphProvider)}.");
            this.httpClient.DefaultRequestHeaders.Authorization = authenticationHeaderValue;
            var requestPayLoad = JsonConvert.SerializeObject(attachment, this.jsonSerializerSettings);

            HttpResponseMessage response = null;
            response = await this.httpClient.PostAsync(
               $"{this.mSGraphSetting.BaseUrl}/{this.mSGraphSetting.GraphAPIVersion}/{this.mSGraphSetting.SendInviteUrl}/{eventId}/attachments",
               new StringContent(requestPayLoad, Encoding.UTF8, ApplicationConstants.JsonMIMEType)).ConfigureAwait(false);

            this.logger.TraceInformation($"Method {nameof(this.DeleteMeetingInvite)}: Completed Graph Send Attachment to invite/Event for notificationId {notificationId}");
            var responseData = await GetResponseData(response).ConfigureAwait(false);

            this.logger.TraceInformation($"Finished {nameof(this.SendMeetingInviteAttachment)} method of {nameof(MSGraphProvider)}.");
            return responseData;
        }
    }
}
