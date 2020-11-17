// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Contracts.Models.Web.Request
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Runtime.Serialization;

    /// <summary>
    /// The <see cref="WebNotificationRequestItemsContainer"/> class stores the multiple <see cref="WebNotificationRequestItem"/> objects.
    /// </summary>
    [DataContract]
    public class WebNotificationRequestItemsContainer
    {
        /// <summary>
        /// Gets or sets the notifications.
        /// </summary>
        /// <value>
        /// The notifications.
        /// </value>
        [Required(ErrorMessage = "The notifications are mandatory.")]
        [MinLength(1, ErrorMessage = "There are no notifications.")]
        [MaxLength(10, ErrorMessage = "The notifications should not exceed 10.")]
        [DataMember(Name = "notifications")]
        public List<WebNotificationRequestItem> Notifications { get; set; }
    }
}
