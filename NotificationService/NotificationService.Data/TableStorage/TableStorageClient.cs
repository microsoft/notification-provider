// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Data
{
    using Microsoft.Azure.Cosmos.Table;
    using Microsoft.Extensions.Options;
    using NotificationService.Common;

    /// <summary>
    /// Client Interface to the Azure Cloud Storage.
    /// </summary>
    public class TableStorageClient : ITableStorageClient
    {
        /// <summary>
        /// Instance of <see cref="StorageAccountSetting"/>.
        /// </summary>
        private readonly StorageAccountSetting storageAccountSetting;

        /// <summary>
        /// Instance of <see cref="CloudStorageAccount"/>.
        /// </summary>
        private readonly CloudStorageAccount cloudStorageAccount;

        /// <summary>
        /// Instance of <see cref="CloudTableClient"/>.
        /// </summary>
        private readonly CloudTableClient cloudTableClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="TableStorageClient"/> class.
        /// </summary>
        /// <param name="storageAccountSetting">Storage Account configuration.</param>
        public TableStorageClient(IOptions<StorageAccountSetting> storageAccountSetting)
        {
            this.storageAccountSetting = storageAccountSetting?.Value;
            this.cloudStorageAccount = CloudStorageAccount.Parse(this.storageAccountSetting.ConnectionString);
            this.cloudTableClient = this.cloudStorageAccount.CreateCloudTableClient();
        }

        /// <inheritdoc/>
        public CloudTable GetCloudTable(string tableName)
        {
            CloudTable cloudTable = this.cloudTableClient.GetTableReference(tableName);
            _ = cloudTable.CreateIfNotExists();
            return cloudTable;
        }

        /*  /// <inheritdoc/>
          public Task QueueCloudMessages(CloudQueue cloudQueue, IEnumerable<string> messages, TimeSpan? initialVisibilityDelay = null)
          {
              messages.ToList().ForEach(msg =>
              {
                  CloudQueueMessage message = new CloudQueueMessage(msg);
                  cloudQueue.AddMessage(message, null, initialVisibilityDelay);
              });
              return Task.CompletedTask;
          }*/
    }
}
