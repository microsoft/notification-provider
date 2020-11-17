// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationsQueueProcessor
{
    using System.Net.Http;
    using System.Threading.Tasks;

    /// <summary>
    /// IHttpClientHelper
    /// </summary>
    public interface IHttpClientHelper
    {
        /// <summary>
        /// Posts the asynchronous.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="content">The content.</param>
        /// <returns></returns>
        Task<HttpResponseMessage> PostAsync(string url, HttpContent content);
    }
}
