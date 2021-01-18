// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Contracts.Models.Graph.Invite
{
    using System.Runtime.Serialization;

    /// <summary>
    /// InviteTimeZone Enum.
    /// </summary>
    [DataContract]
    public enum InviteTimeZone
    {
        /// <summary>
        /// InviteTimeZone PST.
        /// </summary>
        [EnumMember(Value = "Pacific Standard Time")]
        PST,

        /// <summary>
        /// InviteTimeZone IST.
        /// </summary>
        [EnumMember(Value = "India Standard Time")]
        IST,

        /// <summary>
        /// InviteTimeZone UTC.
        /// </summary>
        [EnumMember(Value = "UTC")]
        UTC,
    }
}
