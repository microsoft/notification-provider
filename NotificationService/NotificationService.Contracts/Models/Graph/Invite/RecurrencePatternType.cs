// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Contracts.Models.Graph.Invite
{
    using System.Runtime.Serialization;

    /// <summary>
    /// RecurrencePatternType Enum.
    /// </summary>
    [DataContract]
    public enum RecurrencePatternType
    {
        /// <summary>
        /// RecurrencePatternType Daily.
        /// </summary>
        [EnumMember(Value = "daily")]
        Daily,

        /// <summary>
        /// RecurrencePatternType Weekly.
        /// </summary>
        [EnumMember(Value = "weekly")]
        Weekly,

        /// <summary>
        /// RecurrencePatternType AbsoluteMonthly.
        /// </summary>
        [EnumMember(Value = "absoluteMonthly")]
        Monthly,

        /// <summary>
        /// RecurrencePatternType AbsoluteYearly.
        /// </summary>
        [EnumMember(Value = "absoluteYearly")]
        Yearly,

        /// <summary>
        /// RecurrencePatternType RelativeYealry.
        /// </summary>
        [EnumMember(Value = "relativeYearly")]
        RelativeYearly,

        /// <summary>
        /// RecurrencePatternType RelativeMonthly.
        /// </summary>
        [EnumMember(Value = "relativeMonthly")]
        RelativeMonthly,
    }
}
