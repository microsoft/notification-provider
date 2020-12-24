// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Contracts.Models.Graph.Invite
{
    using System.Runtime.Serialization;

    /// <summary>
    /// Location Address Model for Invite.
    /// </summary>
    [DataContract]
    public class Address
    {
        /// <summary>
        /// Gets or Sets Street.
        /// </summary>
        [DataMember(Name = "street")]
        public string Street { get; set; }

        /// <summary>
        /// Gets or Sets City.
        /// </summary>
        [DataMember(Name = "city")]
        public string City { get; set; }

        /// <summary>
        /// Gets or Sets State.
        /// </summary>
        [DataMember(Name = "state")]
        public string State { get; set; }

        /// <summary>
        /// Gets or Sets CountryOrRegion.
        /// </summary>
        [DataMember(Name = "countryOrRegion")]
        public string CountryOrRegion { get; set; }

        /// <summary>
        /// Gets or Sets PostalCode.
        /// </summary>
        [DataMember(Name = "postalCode")]
        public string PostalCode { get; set; }
    }
}
