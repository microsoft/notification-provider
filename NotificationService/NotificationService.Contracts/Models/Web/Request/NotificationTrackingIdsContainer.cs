// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Contracts.Models.Web.Request
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Runtime.Serialization;

    /// <summary>
    /// The <see cref="NotificationTrackingIdsContainer"/> class stores the tracking Ids of notifications.
    /// </summary>
    public class NotificationTrackingIdsContainer
    {
        /// <summary>
        /// Gets or sets the tracking Ids.
        /// </summary>
        /// <value>
        /// The tracking identifiers.
        /// </value>
        [Required(ErrorMessage = "The tracking Ids are not specified.")]
        [MinLength(1, ErrorMessage = "There are no tracking Ids.")]
        [MaxLength(10, ErrorMessage = "The tracking Ids should not exceed 10.")]
        [DataMember(Name = "trackingIds")]
        public List<string> TrackingIds { get; set; }
    }
}
