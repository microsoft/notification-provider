// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Contracts
{
    using System.Runtime.Serialization;

    /// <summary>
    /// Headers of Http request to Graph API.
    /// </summary>
    [DataContract]
    public class GraphRequestHeaders
    {
        /// <summary>
        /// Gets or sets content type of the body of Http request.
        /// </summary>
        [DataMember(Name = "Content-Type")]
        public string ContentType { get; set; }
    }
}
