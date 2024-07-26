// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Common
{
    /// <summary>
    /// Storage Account Configuration Settings.
    /// </summary>
    public class StorageAccountSetting
    {
        /// <summary>
        /// Gets or sets the Blob Container Name.
        /// </summary>
        public string BlobContainerName { get; set; }

        /// <summary>
        /// Gets or sets the Mail template table name.
        /// </summary>
        public string MailTemplateTableName { get; set; }

        /// <summary>
        /// Gets or sets the Email History table name.
        /// </summary>
        public string EmailHistoryTableName { get; set; }

        /// <summary>
        /// Gets or sets the Meeting History table name.
        /// </summary>
        public string MeetingHistoryTableName { get; set; }

        /// <summary>
        /// Gets or sets the Notification queue name.
        /// </summary>
        public string NotificationQueueName { get; set; }
        public string StorageAccountName { get; set; }       
        public string StorageTableAccountURI { get; set; }
        public string StorageBlobAccountURI { get; set; }
        public string StorageQueueAccountURI { get; set; }
    }
}
