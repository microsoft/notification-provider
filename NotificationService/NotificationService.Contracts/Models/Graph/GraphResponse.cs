// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Contracts
{
    using System.ComponentModel.DataAnnotations;
    using System.Net;
    using System.Runtime.Serialization;

    /// <summary>
    /// Http response from Graph API.
    /// </summary>
    [DataContract]
    public class GraphResponse
    {
        /// <summary>
        /// Gets or sets unique identifier of the associated request.
        /// </summary>
        [DataMember(Name = "id")]
        [Required(ErrorMessage = "Unique identifier is mandatory for graph response in batch.")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets status code of the Http Response.
        /// </summary>
        [DataMember(Name = "status")]
        [Required(ErrorMessage = "Status is mandatory for graph request in batch.")]
        public HttpStatusCode Status { get; set; }

        /// <summary>
        /// Gets or sets body of the Http Response.
        /// </summary>
        [DataMember(Name = "body")]
        public GraphResponseBody Body { get; set; }
    }
}
