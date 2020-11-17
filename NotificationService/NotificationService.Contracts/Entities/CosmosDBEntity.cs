// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Contracts
{
    using System;
    using System.Runtime.Serialization;
    using Microsoft.Azure.Cosmos.Table;

    /// <summary>
    /// Cosmos DB Base entity.
    /// </summary>
    [DataContract]
    public abstract class CosmosDBEntity : TableEntity
    {
        /// <summary>Gets or sets the id.</summary>
        [DataMember(Name = "id")]
        public string Id { get; set; }

        /// <summary>Gets or sets the OID that the document was created by.</summary>
        [DataMember(Name = "CreatedBy")]
        public string CreatedBy { get; set; }

        /// <summary>Gets or sets the OID that the document was updated by.</summary>
        [DataMember(Name = "UpdatedBy")]
        public string UpdatedBy { get; set; }

        /// <summary>Gets or sets the created at.</summary>
        [DataMember(Name = "CreatedAt")]
        public long CreatedAt { get; set; }

        /// <summary>Gets or sets the updated at.</summary>
        [DataMember(Name = "UpdatedAt")]
        public long UpdatedAt { get; set; }

        /// <summary>Gets or sets the created at.</summary>
        [DataMember(Name = "CreatedDateTime")]
        public DateTime CreatedDateTime { get; set; }

        /// <summary>Gets or sets the updated at.</summary>
        [DataMember(Name = "UpdatedDateTime")]
        public DateTime UpdatedDateTime { get; set; }
    }
}
