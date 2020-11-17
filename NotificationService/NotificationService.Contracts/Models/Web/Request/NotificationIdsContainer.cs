// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Contracts.Models.Web.Request
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Runtime.Serialization;

    /// <summary>
    /// The <see cref="NotificationIdsContainer"/> class stores the notification Ids of notifications.
    /// </summary>
    public class NotificationIdsContainer
    {
        /// <summary>
        /// Gets or sets the notification Ids.
        /// </summary>
        /// <value>
        /// The notification identifiers.
        /// </value>
        [Required(ErrorMessage = "The notification Ids are not specified.")]
        [MinLength(1, ErrorMessage = "There are no notification Ids.")]
        [MaxLength(10, ErrorMessage = "The notification Ids should not exceed 10.")]
        [DataMember(Name = "notificationIds")]
        public List<string> NotificationIds { get; set; }
    }
}
