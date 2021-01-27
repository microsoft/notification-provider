// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Contracts.Models.Graph.Invite
{
    using System.Runtime.Serialization;

    /// <summary>
    /// AttendeeType Enum.
    /// </summary>
    [DataContract]
    public enum AttendeeType
    {
        /// <summary>
        /// AttendeeType Required.
        /// </summary>
        [EnumMember(Value = "required")]
        Required,

        /// <summary>
        /// AttendeeType Optional
        /// </summary>
        [EnumMember(Value = "optional")]
        Optional,

        /// <summary>
        /// AttendeeType Resource.
        /// </summary>
        [EnumMember(Value = "resource")]
        Resource,
    }
}
