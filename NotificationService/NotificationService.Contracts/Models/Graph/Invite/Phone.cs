// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Contracts.Models.Graph.Invite
{
    using System.Runtime.Serialization;

    /// <summary>
    /// Phone Model.
    /// </summary>
    [DataContract]
    public class Phone
    {
        /// <summary>
        /// Gets or Sets Number.
        /// </summary>
        [DataMember(Name = "number")]
        public string Number { get; set; }

        /// <summary>
        /// Gets or Sets PhoneType.
        /// </summary>
        [DataMember(Name = "type")]
        public PhoneType Type { get; set; }
    }
}
