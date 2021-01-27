// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Contracts.Models.Graph.Invite
{
    using System.Runtime.Serialization;

    /// <summary>
    /// RecurrenceRange Model.
    /// </summary>
    [DataContract]
    public class RecurrenceRange
    {
        /// <summary>
        /// Gets or Sets Type.
        /// </summary>
        [DataMember(Name = "type")]
        public RecurrenceRangeType Type { get; set; }

        /// <summary>
        /// Gets or Sets StartDate in format ("2017-09-04").
        /// </summary>
        [DataMember(Name = "startDate")]
        public string StartDate { get; set; }

        /// <summary>
        /// Gets or Sets EndDate in format ("2017-09-04").
        /// Required in case the type is "endDate".
        /// </summary>
        [DataMember(Name = "endDate")]
        public string EndDate { get; set; }

        /// <summary>
        /// Gets or Sets NumberOfOccurences.
        /// Required in case of type is "numbered".
        /// </summary>
        [DataMember(Name = "numberOfOccurrences")]
        public int? NumberOfOccurences { get; set; }

        /// <summary>
        /// Gets or Sets RecurrenceTimeZone.
        /// Required in case of type is "numbered".
        /// </summary>
        [DataMember(Name = "recurrenceTimeZone")]
        public int? RecurrenceTimeZone { get; set; }
    }
}
