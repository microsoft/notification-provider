// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Contracts
{
    using System.ComponentModel.DataAnnotations;
    using System.Runtime.Serialization;

    /// <summary>
    /// Error details of Http response from Graph API.
    /// </summary>
    [DataContract]
    public class GraphResponseError
    {
        /// <summary>
        /// Gets or sets error code.
        /// </summary>
        [DataMember(Name = "code")]
        [Required(ErrorMessage = "Code is mandatory for graph error response in batch.")]
        public string Code { get; set; }

        /// <summary>
        /// Gets or sets error message.
        /// </summary>
        [DataMember(Name = "message")]
        [Required(ErrorMessage = "Message is mandatory for graph error response in batch.")]
        public string Messsage { get; set; }
    }
}
