// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Contracts.Models.Graph.Invite
{
    using System.Runtime.Serialization;

    /// <summary>
    /// Location Model for invite.
    /// </summary>
    [DataContract]
    public class Location
    {
        /// <summary>
        /// Gets or Sets DisplayName.
        /// </summary>
        [DataMember(Name = "displayName")]
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or Sets Locationtype.
        /// </summary>
        [DataMember(Name = "locationType")]
        public LocationType LocationType { get; set; }

        /// <summary>
        /// Gets or Sets Address.
        /// </summary>
        [DataMember(Name = "Address")]
        public Address Address { get; set; }

        /// <summary>
        /// Gets or Sets LocationEmailAddress.
        /// </summary>
        [DataMember(Name = "locationEmailAddress")]
        public string LocationEmailAddress { get; set; }

        /// <summary>
        /// Gets or Sets LocationUri.
        /// </summary>
        [DataMember(Name = "locationUri")]
        public string LocationUri { get; set; }

        /// <summary>
        /// Gets or Sets UniqueId.
        /// </summary>
        [DataMember(Name = "uniqueId")]
        public string UniqueId { get; set; }

        /// <summary>
        /// Gets or Sets UniqueType.
        /// </summary>
        [DataMember(Name = "uniqueIdType")]
        public string UniqueType { get; set; }
    }
}
