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
        /// Gets or sets Container name.
        /// </summary>
        public string Container { get; set; }
    }
}
