// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Contracts.Models
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Model object for getting date range for resend notifications.
    /// </summary>
    public class DateTimeRange
    {
        /// <summary>
        /// Gets or Sets Start DateTime.
        /// </summary>
        [DataMember(Name ="startDate")]
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Gets or Sets End DateTime.
        /// </summary>
        [DataMember(Name="endDate")]
        public DateTime EndDate { get; set; }
    }
}
