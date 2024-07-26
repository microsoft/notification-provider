// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Data
{
    using Azure;
    using Azure.Data.Tables;
    using Microsoft.Azure.Cosmos.Serialization.HybridRow.RecordIO;
    using Microsoft.Extensions.Options;
    using NotificationService.Common;
    using NotificationService.Contracts.Models;
    using NotificationService.Data.Helper;
    using System;
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Client Interface to the Azure Cloud Storage.
    /// </summary>
    public class TableStorageClient : ITableStorageClient
    {
        /// <summary>
        /// Instance of <see cref="StorageAccountSetting"/>.
        /// </summary>
        private readonly StorageAccountSetting storageAccountSetting;

        private static ConcurrentDictionary<string, TableServiceClient> _tableServiceClientRefs = new ConcurrentDictionary<string, TableServiceClient>();
        private ConcurrentDictionary<string, TableClient> _tableClientRefs;
        private string _storageTableAccountUri;

        /// <summary>
        /// Initializes a new instance of the <see cref="TableStorageClient"/> class.
        /// </summary>
        /// <param name="storageAccountUri"></param>
        public TableStorageClient(IOptions<StorageAccountSetting> storageAccountSetting)
        {
            this.storageAccountSetting = storageAccountSetting?.Value;
            _tableClientRefs = new ConcurrentDictionary<string, TableClient>();
            _storageTableAccountUri = this.storageAccountSetting.StorageTableAccountURI;
        }


        /// <summary>
        /// Retrieves a reference to table storage account via the URI of storage account.
        /// </summary>
        /// <param name="storageAccountUri">Storage Account URI</param>
        /// <returns>It returns the reference to the table service client.</returns>
        public TableServiceClient GetTableServiceClient(string storageAccountUri)
        {
            TableServiceClient? tableServiceClient;
            if (!_tableServiceClientRefs.TryGetValue(storageAccountUri, out tableServiceClient))
            {
                tableServiceClient = new TableServiceClient(new Uri(storageAccountUri), AzureCredentialHelper.AzureCredentials);
                _tableServiceClientRefs.TryAdd(storageAccountUri, tableServiceClient);
            }

            return tableServiceClient;
        }

        /// <summary>
        /// Retrieves a reference to a table client with the name from the storage account. If the table does not exist, it creates the table before returning the reference.
        /// </summary>
        /// <param name="tableServiceClient">TableServiceClient</param>
        /// <param name="tableName">Storage table name</param>
        /// <returns>It returns the reference to the table client.</returns>
        public TableClient GetTableClient(TableServiceClient tableServiceClient, string tableName)
        {
            TableClient? tableClient;
            if (!_tableClientRefs.TryGetValue($"{tableServiceClient.AccountName}-{tableName}", out tableClient))
            {
                _ = tableServiceClient.CreateTableIfNotExists(tableName);
                tableClient = tableServiceClient.GetTableClient(tableName);
                _tableClientRefs.TryAdd($"{tableServiceClient.AccountName}-{tableName}", tableClient);
            }

            return tableClient;
        }

        /// <summary>
        /// Add or Update a table entity into sepcified table name
        /// </summary>
        /// <typeparam name="T">Type of row</typeparam>
        /// <param name="tableName">Storage table name</param>
        /// <param name="entity">Entity to Add/Update</param>
        /// <returns></returns>
        public async Task<bool> AddOrUpdateAsync<T>(string tableName, T entity) where T : TableEntityBase
        {
            var tableServiceClient = GetTableServiceClient(_storageTableAccountUri);
            var tableClient = GetTableClient(tableServiceClient, tableName);

            await tableClient.UpsertEntityAsync(entity);

            return true;
        }

        /// <summary>
        /// Add or Update a table entity into sepcified table name
        /// </summary>
        /// <typeparam name="T">Type of row</typeparam>
        /// <param name="tableName">Storage table name</param>
        /// <param name="entity">Entity to Add/Update</param>
        /// <returns></returns>
        public async Task<bool> AddsOrUpdatesAsync<T>(string tableName, List<T> entity) where T : TableEntityBase
        {
            var tableServiceClient = GetTableServiceClient(_storageTableAccountUri);
            var tableClient = GetTableClient(tableServiceClient, tableName);
                      

            foreach (T record in entity)
            {
                await tableClient.UpsertEntityAsync(record);
            }

            return true;
        }

        /// <summary>
        /// Inserts a list of records into sepcified table name
        /// </summary>
        /// <typeparam name="T">Type of row</typeparam>
        /// <param name="tableName">Storage table name</param>
        /// <param name="records">List of rows</param>
        /// <returns></returns>
        public async Task<bool> InsertRecordsAsync<T>(string tableName, List<T> records) where T : TableEntityBase
        {
            var tableServiceClient = GetTableServiceClient(_storageTableAccountUri);
            var tableClient = GetTableClient(tableServiceClient, tableName);
            foreach (T record in records)
            {
                await tableClient.AddEntityAsync(record);
            }

            return true;
        }

        /// <summary>
        /// Fetches a List of records from Azure Storage table based on filter query
        /// </summary>
        /// <typeparam name="T">Type of row</typeparam>
        /// <param name="tableName">Storage table name</param>
        /// <param name="filter">Query to Filter rows</param>
        /// <returns></returns>
        public async Task<List<T>> GetRecordsAsync<T>(string tableName, string filter) where T : TableEntityBase
        {
            List<T> results = new List<T>();

            var tableServiceClient = GetTableServiceClient(_storageTableAccountUri);
            var tableClient = GetTableClient(tableServiceClient, tableName);

            AsyncPageable<T> queryResultsFilter = tableClient.QueryAsync<T>(filter: filter);

            await foreach (T qEntity in queryResultsFilter)
            {
                results.Add(qEntity);
            }

            return results;
        }        

        /// <summary>
        /// Fetches a specific row from Azure Storage table based on partition key and row key
        /// </summary>
        /// <typeparam name="T">Type of row</typeparam>
        /// <param name="tableName">Storage table name</param>
        /// <param name="partitionKey">Partition Key</param>
        /// <param name="rowKey">Row Key</param>
        /// <returns></returns>
        public async Task<T?> GetRecordAsync<T>(string tableName, string partitionKey, string rowKey) where T : TableEntityBase
        {
            var tableServiceClient = GetTableServiceClient(_storageTableAccountUri);
            var tableClient = GetTableClient(tableServiceClient, tableName);

            var result = await tableClient.GetEntityIfExistsAsync<T>(partitionKey, rowKey);

            if (result.HasValue && result.Value != null)
            {
                return result.Value;
            }
            else
            {
                return default;
            }
        }

        /// <summary>
        /// Deletes a specific row from Azure Storage table based on partition key and row key
        /// </summary>
        /// <typeparam name="T">Type of row</typeparam>
        /// <param name="tableName">Storage table name</param>
        /// <param name="partitionKey">Partition Key</param>
        /// <param name="rowKey">Row Key</param>
        /// <returns></returns>
        public async Task<bool> DeleteRecordAsync(string tableName, string partitionKey, string rowKey)
        {
            var tableServiceClient = GetTableServiceClient(_storageTableAccountUri);
            var tableClient = GetTableClient(tableServiceClient, tableName);

            await tableClient.DeleteEntityAsync(rowKey, partitionKey);

            return true;
        }
    }
}
