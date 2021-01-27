// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Contracts.Models.Graph.Invite
{
    using System.Runtime.Serialization;

    /// <summary>
    /// ImportanceType Enum.
    /// </summary>
    [DataContract]
    public enum ImportanceType
    {
        /// <summary>
        /// ImportanceType High.
        /// </summary>
        [EnumMember(Value = "high")]
        High,

        /// <summary>
        /// ImportanceType Normal.
        /// </summary>
        [EnumMember(Value = "normal")]
        Normal,

        /// <summary>
        /// ImportanceType Low.
        /// </summary>
        [EnumMember(Value = "low")]
        Low,
    }
}
