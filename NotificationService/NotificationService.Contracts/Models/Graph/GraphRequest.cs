// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Contracts
{
    using System.ComponentModel.DataAnnotations;
    using System.Runtime.Serialization;

    /// <summary>
    /// Http request to Graph API.
    /// </summary>
    [DataContract]
    public class GraphRequest
    {
        /// <summary>
        /// Gets or sets unique identifier of the request.
        /// </summary>
        [DataMember(Name = "id")]
        [Required(ErrorMessage = "Unique identifier is mandatory for graph request in batch.")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets verb of the Http Request.
        /// </summary>
        [DataMember(Name = "method")]
        [Required(ErrorMessage = "Method/Verb is mandatory for graph request in batch.")]
        public string Method { get; set; }

        /// <summary>
        /// Gets or sets url of the Http Request.
        /// </summary>
        [DataMember(Name = "url")]
        [Required(ErrorMessage = "Url is mandatory for graph request in batch.")]
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets body of the Http Request.
        /// </summary>
        [DataMember(Name = "body")]
        public object Body { get; set; }

        /// <summary>
        /// Gets or sets headers for the Http Request.
        /// </summary>
        [DataMember(Name = "headers")]
        public GraphRequestHeaders Headers { get; set; }
    }
}
