// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.BusinessLibrary
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.ServiceModel.Channels;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json;
    using NotificationService.Common;
    using NotificationService.Common.Logger;
    using NotificationService.Contracts;
    using Polly;
    using Polly.Extensions.Http;
    using Polly.Retry;

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
            $"{BusinessConstants.GraphBaseUrl}/{this.mSGraphSetting.GraphAPIVersion}/{this.mSGraphSetting.BatchRequestUrl}",
            new StringContent(requestPayLoad, Encoding.UTF8, Constants.JsonMIMEType)).ConfigureAwait(false);

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
        public async Task<bool> SendEmailNotification(AuthenticationHeaderValue authenticationHeaderValue, EmailMessagePayload payLoad, string notificationId)
        {
            this.logger.TraceInformation($"Started {nameof(this.SendEmailNotification)} method of {nameof(MSGraphProvider)}.");
            this.httpClient.DefaultRequestHeaders.Authorization = authenticationHeaderValue;
            var requestPayLoad = JsonConvert.SerializeObject(payLoad, this.jsonSerializerSettings);
            HttpResponseMessage response = null;
            bool isSuccess = false;
            response = await this.httpClient.PostAsync(
                    $"{BusinessConstants.GraphBaseUrl}/{this.mSGraphSetting.GraphAPIVersion}/{this.mSGraphSetting.SendMailUrl}",
                    new StringContent(requestPayLoad, Encoding.UTF8, Constants.JsonMIMEType)).ConfigureAwait(false);

            this.logger.TraceInformation($"Method {nameof(this.SendEmailNotification)}: Completed Graph Send Email Call.");
            var responseHeaders = response.Headers.ToString();

            if (response.IsSuccessStatusCode)
            {
                // Read and deserialize response.
                var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                isSuccess = true;
            }
            else if (response.StatusCode == HttpStatusCode.TooManyRequests || response.StatusCode == HttpStatusCode.RequestTimeout)
            {
                isSuccess = false;
            }
            else
            {
                string content = string.Empty;
                if (response != null)
                {
                    content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                }

                throw new System.Exception($"An error occurred while sending notification id: {notificationId}. Details: {content}");
            }

            this.logger.TraceInformation($"Finished {nameof(this.SendEmailNotification)} method of {nameof(MSGraphProvider)}.");
            return isSuccess;
        }
    }
}
