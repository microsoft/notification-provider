// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Contracts
{
    using System.ComponentModel.DataAnnotations;
    using System.Runtime.Serialization;

    /// <summary>
    /// Body of Http response from Graph API.
    /// </summary>
    [DataContract]
    public class GraphResponseBody
    {
        /// <summary>
        /// Gets or sets error details if the associated request failed.
        /// </summary>
        [DataMember(Name = "error")]
        [Required(ErrorMessage = "Error is mandatory for graph response body.")]
        public GraphResponseError Error { get; set; }
    }
}
