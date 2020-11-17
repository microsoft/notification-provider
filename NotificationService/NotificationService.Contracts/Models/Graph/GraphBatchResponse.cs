// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Contracts
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.Serialization;

    /// <summary>
    /// Batch response from Graph API.
    /// </summary>
    [ExcludeFromCodeCoverage]
    [DataContract]
    public class GraphBatchResponse
    {
        /// <summary>
        /// Gets or sets responses for the requests in Batch.
        /// </summary>
        [DataMember(Name = "responses")]
        [Required(ErrorMessage = "Responses collection is mandatory for graph batch response.")]
        public List<GraphResponse> Responses { get; set; }
    }
}
