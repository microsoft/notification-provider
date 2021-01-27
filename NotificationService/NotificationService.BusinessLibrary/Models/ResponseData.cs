// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.BusinessLibrary.Models
{
    using System.Net;

    /// <summary>
    /// Capture Response from API calls.
    /// </summary>
    /// <typeparam name="T">Type parameter for Result.</typeparam>
    public class ResponseData<T>
    {
        /// <summary>
        /// Gets or sets a value indicating whether gets or Sets value for success/failed.
        /// </summary>
        public bool Status { get; set; }

        /// <summary>
        /// Gets or Sets Result.
        /// </summary>
        public T Result { get; set; }

        /// <summary>
        /// Gets or Sets ErrorMessage.
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Gets or Sets StatusCode.
        /// </summary>
        public HttpStatusCode StatusCode { get; set; }
    }
}
