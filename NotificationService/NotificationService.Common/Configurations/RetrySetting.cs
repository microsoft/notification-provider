// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Common
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Retry Configuration Settings.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class RetrySetting
    {
        /// <summary>
        /// Gets or sets Max Retries.
        /// </summary>
        public int MaxRetries { get; set; }

        /// <summary>
        /// Gets or sets TransientRetryCount.
        /// </summary>
        public int TransientRetryCount { get; set; }
    }
}
