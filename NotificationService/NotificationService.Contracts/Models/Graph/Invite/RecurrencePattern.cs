// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Contracts.Models.Graph.Invite
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    /// <summary>
    /// RecurrencePattern Model.
    /// </summary>
    [DataContract]
    public class RecurrencePattern
    {
        /// <summary>
        /// Gets or Sets RecurrencePatterType.
        /// </summary>
        [DataMember(Name = "type", IsRequired = true)]
        public RecurrencePatternType Type { get; set; }

        /// <summary>
        /// Gets or Sets Inverval.
        /// </summary>
        [DataMember(Name = "interval", IsRequired = true)]
        public int? Interval { get; set; }

        /// <summary>
        /// Gets or Sets DaysOfWeek.
        /// values [ "Monday" ]
        /// required in case of weekly type interval == daysofweek.count.
        /// </summary>
        [DataMember(Name = "daysOfWeek")]
        public IList<DayOfTheWeek> DaysOfWeek { get; set; }

        /// <summary>
        /// Gets or Sets DayOfMonth.
        /// </summary>
        [DataMember(Name = "dayOfMonth")]
        public int? DayOfMonth { get; set; }

        /// <summary>
        /// Gets or Sets Index.
        /// which day of the month.
        /// used with Monthly Type.
        /// like if daysofWeek is "Wednesday" and index= second => second Wednesday of the month.
        /// </summary>
        [DataMember(Name = "index")]
        public string Index { get; set; }

        /// <summary>
        /// Gets or Sets Month.
        /// Used with Yearly Types.
        /// same like index => define month of the year.
        /// </summary>
        [DataMember(Name = "month")]
        public int? Month { get; set; }

        /// <summary>
        /// Gets or Sets FirstDayOfWeek.
        /// this asks for a day which need to be set to first day of the week  (default : Sunday).
        /// </summary>
        [DataMember(Name = "firstDayOfWeek")]
        public string FirstDayOfWeek { get; set; }
    }
}
