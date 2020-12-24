// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Contracts.Models.Graph.Invite
{
    using System.Runtime.Serialization;

    /// <summary>
    /// RecurrenceRangeType Enum.
    /// </summary>
    [DataContract]
    public enum RecurrenceRangeType
    {
        /// <summary>
        /// RecurrenceRangeType Numbered.
        /// given number of times the event will occur.
        /// </summary>
        [EnumMember(Value = "numbered")]
        Numbered,

        /// <summary>
        /// RecurrenceRangeType EndDate.
        /// ends with enddate.
        /// </summary>
        [EnumMember(Value = "endDate")]
        EndDate,

        /// <summary>
        /// RecurrenceRangeType NoEnd.
        ///  never end after starting on start Date.
        /// </summary>
        [EnumMember(Value = "noEnd")]
        NoEnd,
    }
}
