// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Contracts.Entities
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.Azure.Cosmos.Table;

    /// <summary>
    /// Storage Entity to store EmailId and NotificationId mapping for GDPR implementation.
    /// </summary>
    public class EmailNotificationMapEntity : TableEntity
    {
        /// <summary>
        /// Gets or Sets ApplicationName.
        /// </summary>
        public string ApplicationName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether gets or Sets scrubbed status.
        /// </summary>
        public bool IsScrubbed { get; set; }

        /// <summary>
        /// Gets or Sets CreateDateTime.
        /// </summary>
        public DateTime CreateDateTime { get; set; }

        /// <summary>
        /// Gets or Sets UpdateDateTime.
        /// </summary>
        public DateTime UpdateDateTime { get; set; }

        /// <summary>
        /// Comparer for EmailNotificaitonMapEntity.
        /// </summary>
        public class EmailNotificaitonMapEntityComparer : IEqualityComparer<EmailNotificationMapEntity>
        {
            /// <summary>
            /// Equals comparison of two objects.
            /// </summary>
            /// <param name="obj1">first object to compare.</param>
            /// <param name="obj2">second object to compare.</param>
            /// <returns>Returns bool value as comparision result.</returns>
            public bool Equals(EmailNotificationMapEntity obj1, EmailNotificationMapEntity obj2)
            {
                return obj1 != null && obj2 != null && obj1.RowKey == obj2.RowKey && obj1.PartitionKey == obj2.PartitionKey;
            }

            /// <summary>
            /// Generate HashCode for given Object.
            /// </summary>
            /// <param name="obj">Object for which hashcode is generated.</param>
            /// <returns>Returns hascode for the input object.</returns>
            public int GetHashCode([DisallowNull] EmailNotificationMapEntity obj)
            {
                if (obj == null)
                {
                    return 0;
                }

                int hash = 17;
                if (!string.IsNullOrEmpty(obj.PartitionKey))
                {
                    hash = (hash * 23) ^ obj.PartitionKey.GetHashCode(StringComparison.InvariantCulture);
                }

                if (!string.IsNullOrEmpty(obj.RowKey))
                {
                    hash = (hash * 23) ^ obj.RowKey.GetHashCode(StringComparison.InvariantCulture);
                }

                return hash;
            }
        }
    }
}
