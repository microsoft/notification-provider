// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Data
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Azure.Cosmos.Table;
    using Microsoft.Extensions.Options;
    using NotificationService.Common;
    using NotificationService.Common.Logger;
    using NotificationService.Contracts;
    using NotificationService.Contracts.Entities;
    using NotificationService.Contracts.Extensions;
    using NotificationService.Data.Interfaces;

    /// <summary>
    /// Repository for mail templates.
    /// </summary>
    public class MailTemplateRepository : IMailTemplateRepository
    {
        private readonly ILogger logger;
        private readonly ICloudStorageClient cloudStorageClient;
        private readonly ITableStorageClient tableStorageClient;
        private readonly CloudTable cloudTable;

        /// <summary>
        /// Initializes a new instance of the <see cref="MailTemplateRepository"/> class.
        /// </summary>
        /// <param name="logger">logger.</param>
        /// <param name="cloudStorageClient">cloud storage client for blob storage.</param>
        /// <param name="tableStorageClient">cloud storage client for table storage.</param>
        /// <param name="storageAccountSetting">primary key of storage account.</param>
        public MailTemplateRepository(
            ILogger logger,
            ICloudStorageClient cloudStorageClient,
            ITableStorageClient tableStorageClient,
            IOptions<StorageAccountSetting> storageAccountSetting)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.cloudStorageClient = cloudStorageClient ?? throw new ArgumentNullException(nameof(cloudStorageClient));
            this.tableStorageClient = tableStorageClient ?? throw new ArgumentNullException(nameof(tableStorageClient));

            if (storageAccountSetting is null)
            {
                throw new ArgumentNullException(nameof(storageAccountSetting));
            }

            if (string.IsNullOrWhiteSpace(storageAccountSetting?.Value?.MailTemplateTableName))
            {
                this.logger.WriteException(new ArgumentException("MailTemplateTableName"));
                throw new ArgumentException("MailTemplateTableName");
            }

            this.cloudTable = this.tableStorageClient.GetCloudTable(storageAccountSetting.Value.MailTemplateTableName);
        }

        /// <inheritdoc/>
        public async Task<MailTemplateEntity> GetMailTemplate(string applicationName, string templateName)
        {
            this.logger.TraceInformation($"Started {nameof(this.GetMailTemplate)} method of {nameof(MailTemplateRepository)}.");

            string blobName = this.GetBlobName(applicationName, templateName);
            var contentTask = this.cloudStorageClient.DownloadBlobAsync(blobName).ConfigureAwait(false);

            TableOperation retrieveOperation = TableOperation.Retrieve<MailTemplateEntity>(applicationName, templateName);

            TableResult retrievedResult = await this.cloudTable.ExecuteAsync(retrieveOperation).ConfigureAwait(false);

            MailTemplateEntity templateEntity = retrievedResult?.Result as MailTemplateEntity;

            if (templateEntity != null)
            {
                templateEntity.Content = await contentTask;
            }

            this.logger.TraceInformation($"Finished {nameof(this.GetMailTemplate)} method of {nameof(MailTemplateRepository)}.");

            return templateEntity;
        }

        /// <inheritdoc/>
        public async Task<IList<MailTemplateEntity>> GetAllTemplateEntities(string applicationName)
        {
            this.logger.TraceInformation($"Started {nameof(this.GetAllTemplateEntities)} method of {nameof(MailTemplateRepository)}.");

            List<MailTemplateEntity> mailTemplateEntities = new List<MailTemplateEntity>();
            var filterPartitionkey = TableQuery.GenerateFilterCondition(nameof(MailTemplateEntity.PartitionKey), QueryComparisons.Equal, applicationName);
            var query = new TableQuery<MailTemplateEntity>().Where(filterPartitionkey);
            TableContinuationToken continuationToken = null;
            do
            {
                var page = await this.cloudTable.ExecuteQuerySegmentedAsync(query, continuationToken).ConfigureAwait(false);
                continuationToken = page.ContinuationToken;
                mailTemplateEntities.AddRange(page.Results);
            }
            while (continuationToken != null);

            this.logger.TraceInformation($"Finished {nameof(this.GetAllTemplateEntities)} method of {nameof(MailTemplateRepository)}.");

            return mailTemplateEntities;
        }

        /// <inheritdoc/>
        public async Task<bool> UpsertEmailTemplateEntities(MailTemplateEntity mailTemplateEntity)
        {
            bool result = false;
            this.logger.TraceInformation($"Started {nameof(this.UpsertEmailTemplateEntities)} method of {nameof(MailTemplateRepository)}.");

            if (mailTemplateEntity is null)
            {
                throw new ArgumentNullException(nameof(mailTemplateEntity));
            }

            string blobName = this.GetBlobName(mailTemplateEntity.Application, mailTemplateEntity.TemplateName);
            string blobUri = await this.cloudStorageClient.UploadBlobAsync(
                blobName,
                mailTemplateEntity.Content)
                .ConfigureAwait(false);

            // Making sure content is not stored in table storage
            mailTemplateEntity.Content = null;

            // Create the TableOperation object that inserts the entity.
            TableOperation insertOperation = TableOperation.InsertOrReplace(mailTemplateEntity);
            var response = await this.cloudTable.ExecuteAsync(insertOperation).ConfigureAwait(false);
            result = true;
            this.logger.TraceInformation($"Finished {nameof(this.UpsertEmailTemplateEntities)} method of {nameof(MailTemplateRepository)}.");

            return result;
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteMailTemplate(string applicationName, string templateName)
        {
            this.logger.TraceInformation($"Started {nameof(this.DeleteMailTemplate)} method of {nameof(MailTemplateRepository)}.");
            bool result = false;
            string blobName = this.GetBlobName(applicationName, templateName);
            var status = await this.cloudStorageClient.DeleteBlobsAsync(blobName).ConfigureAwait(false);

            if (status)
            {
                // Retrieve the entity to be deleted.
                TableOperation retrieveOperation = TableOperation.Retrieve<MailTemplateEntity>(applicationName, templateName);
                TableResult tableResult = await this.cloudTable.ExecuteAsync(retrieveOperation).ConfigureAwait(false);
                MailTemplateEntity mailTemplateEntity = tableResult.Result as MailTemplateEntity;
                // Create the TableOperation object that Delets the entity.
                TableOperation deleteOperation = TableOperation.Delete(mailTemplateEntity);
                var response = await this.cloudTable.ExecuteAsync(deleteOperation).ConfigureAwait(false);
                result = true;
            }

            this.logger.TraceInformation($"Finished {nameof(this.DeleteMailTemplate)} method of {nameof(MailTemplateRepository)}.");

            return result;
        }

        /// <summary>
        /// Gets blob name.
        /// </summary>
        /// <param name="applicationName">Application sourcing the email template.</param>
        /// <param name="templateName">Mail template name.</param>
        /// <returns>Blob name.</returns>
        private string GetBlobName(string applicationName, string templateName)
        {
            return $"{applicationName}/{templateName}";
        }
    }
}
