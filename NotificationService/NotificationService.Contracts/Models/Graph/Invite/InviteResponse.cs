// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Contracts.Models.Graph.Invite
{
    using System.Runtime.Serialization;

    /// <summary>
    /// Invite Response Model.
    /// </summary>
    [DataContract]
    public class InviteResponse
    {
        /// <summary>
        /// Gets or Sets Even Id for the invite Request.
        /// </summary>
        [DataMember(Name = "id")]
        public string EventId { get; set; }
    }
}
