// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Contracts.Models.Graph.Invite
{
    using System.Runtime.Serialization;

    /// <summary>
    /// Attendee Model for invite.
    /// </summary>
    [DataContract]
    public class Attendee
    {
        /// <summary>
        /// Gets or Sets EmailAddress.
        /// </summary>
        [DataMember(Name = "emailAddress", IsRequired = true)]
        public EmailAddress EmailAddress { get; set; }

        /// <summary>
        /// Gets or Sets Attendee Type.
        /// </summary>
        [DataMember(Name = "type")]
        public AttendeeType Type { get; set; } = AttendeeType.Optional;
    }
}
