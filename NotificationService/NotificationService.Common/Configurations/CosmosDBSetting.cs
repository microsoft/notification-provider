// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Common
{
    /// <summary>
    /// Cosmos DB Configuration Settings.
    /// </summary>
    public class CosmosDBSetting
    {
        /// <summary>
        /// Gets or sets Uri.
        /// </summary>
        public string Uri { get; set; }

        /// <summary>
        /// Gets or sets Key.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets Database name.
        /// </summary>
        public string Database { get; set; }

        /// <summary>
        /// Gets or sets EmailHistoryContainer name.
        /// </summary>
        public string EmailHistoryContainer { get; set; }

        /// <summary>
        /// Gets or sets MeetingHistoryContainer name.
        /// </summary>
        public string MeetingHistoryContainer { get; set; }
    }
}
