// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Contracts.Models.Graph.Invite
{
    using System.Runtime.Serialization;

    /// <summary>
    /// LocationType Enum.
    /// </summary>
    [DataContract]
    public enum LocationType
    {
        /// <summary>
        /// LocationType Default.
        /// </summary>
        [EnumMember(Value = "default")]
        Default,

        /// <summary>
        /// LocationType ConferenceRoom.
        /// </summary>
        [EnumMember(Value = "conferenceRoom")]
        ConferenceRoom,

        /// <summary>
        /// LocationType HomeAddress.
        /// </summary>
        [EnumMember(Value = "homeAddress")]
        HomeAddress,

        /// <summary>
        /// LocationType BusinessAddress.
        /// </summary>
        [EnumMember(Value = "businessAddress")]
        BusinessAddress,

        /// <summary>
        /// LocationType GeoCoordinates.
        /// </summary>
        [EnumMember(Value = "geoCoordinates")]
        GeoCoordinates,

        /// <summary>
        /// LocationType PostalAddress.
        /// </summary>
        [EnumMember(Value = "postalAddress")]
        PostalAddress,

        /// <summary>
        /// LocationType LocalBusiness.
        /// </summary>
        [EnumMember(Value = "localBusiness")]
        LocalBusiness,

        /// <summary>
        /// LocationType Hotel.
        /// </summary>
        [EnumMember(Value = "hotel")]
        Hotel,

        /// <summary>
        /// LocationType Restaurant.
        /// </summary>
        [EnumMember(Value = "restaurant")]
        Restaurant,

        /// <summary>
        /// LocationType StreetAddress.
        /// </summary>
        [EnumMember(Value = "streetAddress")]
        StreetAddress,
    }
}
