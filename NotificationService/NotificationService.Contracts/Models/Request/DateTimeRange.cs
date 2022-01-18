// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Contracts.Models.Request
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Runtime.Serialization;

    /// <summary>
    /// Model object for getting date range for resend notifications.
    /// </summary>
    public class DateTimeRange
    {
        /// <summary>
        /// Gets or Sets Start DateTime.
        /// </summary>
        [Required(ErrorMessage = "StartDate is a mandatory parameter.")]
        [DataMember(Name = "startDate")]
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Gets or Sets End DateTime.
        /// </summary>
        [Required(ErrorMessage = "EndDate is a mandatory parameter.")]
        [DataMember(Name = "endDate")]
        public DateTime EndDate { get; set; }
    }
}
