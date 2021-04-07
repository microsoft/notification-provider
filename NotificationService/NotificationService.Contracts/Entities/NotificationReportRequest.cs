// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Contracts
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Microsoft.Azure.Cosmos.Table;

    /// <summary>
    /// Base class for Notification Items.
    /// </summary>
    [DataContract]
    public class NotificationReportRequest
    {
        /// <summary>
        /// Gets or sets NotificationPriorityFilter.
        /// </summary>
        [DataMember(Name = "NotificationPriorityFilter")]
        public IList<NotificationPriority> NotificationPriorityFilter { get; set; }

        /// <summary>
        /// Gets or sets ApplicationFilter.
        /// </summary>
        [DataMember(Name = "ApplicationFilter")]
        public IList<string> ApplicationFilter { get; set; }

        /// <summary>
        /// Gets or sets NotificationIdFilter.
        /// </summary>
        [DataMember(Name = "NotificationIdFilter")]
        public IList<string> NotificationIdsFilter { get; set; }

        /// <summary>
        /// Gets or sets TrackingIdsFilter.
        /// </summary>
        [DataMember(Name = "TrackingIdFilter")]
        public IList<string> TrackingIdsFilter { get; set; }

        /// <summary>
        /// Gets or sets AccountUsedFilter.
        /// </summary>
        [DataMember(Name = "AccountUsedFilter")]
        public IList<string> AccountsUsedFilter { get; set; }

        /// <summary>
        /// Gets or sets NotificationStatusFilter.
        /// </summary>
        [DataMember(Name = "NotificationStatusFilter")]
        public IList<NotificationItemStatus> NotificationStatusFilter { get; set; }

        /// <summary>
        /// Gets or sets CreatedDateTimeStart.
        /// </summary>
        [DataMember(Name = "CreatedDateTimeStart")]
        public string CreatedDateTimeStart { get; set; }

        /// <summary>
        /// Gets or sets CreatedDateTimeEnd.
        /// </summary>
        [DataMember(Name = "CreatedDateTimeEnd")]
        public string CreatedDateTimeEnd { get; set; }

        /// <summary>
        /// Gets or sets UpdatedDateTimeStart.
        /// </summary>
        [DataMember(Name = "UpdatedDateTimeStart")]
        public string UpdatedDateTimeStart { get; set; }

        /// <summary>
        /// Gets or sets UpdatedDateTimeEnd.
        /// </summary>
        [DataMember(Name = "UpdatedDateTimeEnd")]
        public string UpdatedDateTimeEnd { get; set; }

        /// <summary>
        /// Gets or sets SendOnUtcDateStart.
        /// </summary>
        [DataMember(Name = "SendOnUtcDateStart")]
        public string SendOnUtcDateStart { get; set; }

        /// <summary>
        /// Gets or sets SendOnUtcDateStart.
        /// </summary>
        [DataMember(Name = "SendOnUtcDateEnd")]
        public string SendOnUtcDateEnd { get; set; }

        /// <summary>
        /// Gets or sets Skip.
        /// </summary>
        [DataMember(Name = "Skip")]
        public int Skip { get; set; }

        /// <summary>
        /// Gets or sets Take.
        /// </summary>
        [DataMember(Name = "Take")]
        public int Take { get; set; }

        /// <summary>
        /// Gets or sets TableContinuationToken.
        /// </summary>
        [DataMember(Name = "Token")]
        public TableContinuationToken Token { get; set; }
    }
}
