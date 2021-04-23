// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace GDPRNotificationMappingProcessor
{

    /// <summary>
    /// class for constant literals.
    /// </summary>
    public sealed class Constants
    {
        /// <summary>
        /// StorageType.
        /// </summary>
        public const string StorageType = "StorageType";

        /// <summary>
        /// Seconds to wait between attempts at polling the Azure KeyVault for changes in configuration.
        /// </summary>
        public const string KeyVaultConfigRefreshDurationSeconds = "KeyVaultConfigRefreshDurationSeconds";
    }
}
