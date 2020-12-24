// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Contracts.Models.Graph.Invite
{
    using System.Runtime.Serialization;

    /// <summary>
    /// InviteDateTime Model.
    /// </summary>
    [DataContract]
    public class InviteDateTime
    {
        /// <summary>
        /// Gets or Sets Datetime in format ("2017-04-15T12:00:00").
        /// </summary>
        [DataMember(Name = "dateTime", IsRequired = true)]
        public string DateTime { get; set; }

        /// <summary>
        /// Gets or Sets Timezone in decriptive formate like ("Pacific Standard Time").
        /// </summary>
        [DataMember(Name = "timeZone")]
        public InviteTimeZone TimeZone { get; set; }
    }
}
