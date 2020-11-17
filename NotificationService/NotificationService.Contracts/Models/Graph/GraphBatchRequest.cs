// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Contracts
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Runtime.Serialization;

    /// <summary>
    /// Batch request to Graph API.
    /// </summary>
    [DataContract]
    public class GraphBatchRequest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GraphBatchRequest"/> class.
        /// </summary>
        public GraphBatchRequest()
        {
            this.Requests = new List<GraphRequest>();
        }

        /// <summary>
        /// Gets or sets list of requests in Batch.
        /// </summary>
        [DataMember(Name = "requests")]
        [Required(ErrorMessage = "Requests collection is mandatory for graph batch request.")]
        public List<GraphRequest> Requests { get; set; }
    }
}
