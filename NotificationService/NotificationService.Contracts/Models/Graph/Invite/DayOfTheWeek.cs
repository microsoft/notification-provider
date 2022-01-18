// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Contracts.Models.Graph.Invite
{
    using System.Runtime.Serialization;

    /// <summary>
    /// DayOfTheWeek Enum.
    /// </summary>
    [DataContract]
    public enum DayOfTheWeek
    {
        /// <summary>
        /// DayOfTheWeek Sunday.
        /// </summary>
        [EnumMember(Value = "sunday")]
        Sunday,

        /// <summary>
        /// DayOfTheWeek Sunday.
        /// </summary>
        [EnumMember(Value = "monday")]
        Monday,

        /// <summary>
        /// DayOfTheWeek Sunday.
        /// </summary>
        [EnumMember(Value = "tuesday")]
        Tuesday,

        /// <summary>
        /// DayOfTheWeek Sunday.
        /// </summary>
        [EnumMember(Value = "wednesday")]
        Wednesday,

        /// <summary>
        /// DayOfTheWeek Sunday.
        /// </summary>
        [EnumMember(Value = "thursday")]
        Thursday,

        /// <summary>
        /// DayOfTheWeek Sunday.
        /// </summary>
        [EnumMember(Value = "friday")]
        Friday,

        /// <summary>
        /// DayOfTheWeek Sunday.
        /// </summary>
        [EnumMember(Value = "saturday")]
        Saturday,
    }
}
