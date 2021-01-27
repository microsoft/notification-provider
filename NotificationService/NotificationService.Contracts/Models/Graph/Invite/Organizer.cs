// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Contracts.Models.Graph.Invite
{
    using System.Runtime.Serialization;

    /// <summary>
    /// Organizer Model.
    /// </summary>
    [DataContract]
    public class Organizer
    {
        /// <summary>
        /// Gets or Sets EmailAddress.
        /// </summary>
        [DataMember(Name = "emailAddress")]
        public EmailAddress EmailAddress { get; set; }
    }
}