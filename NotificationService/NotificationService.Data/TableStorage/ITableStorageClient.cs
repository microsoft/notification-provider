// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Data
{
    using Azure;
    using Azure.Data.Tables;
    using NotificationService.Contracts.Models;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Interface to Azure Cloud Storage.
    /// </summary>
    public interface ITableStorageClient
    {
        TableServiceClient GetTableServiceClient(string storageAccountUri);
        TableClient GetTableClient(TableServiceClient tableServiceClient, string tableName);
        Task<bool> AddOrUpdateAsync<T>(string tableName, T entity) where T : TableEntityBase;
        Task<bool> AddsOrUpdatesAsync<T>(string tableName, List<T> entity) where T : TableEntityBase;
        Task<bool> InsertRecordsAsync<T>(string tableName, List<T> records) where T : TableEntityBase;
        Task<List<T>> GetRecordsAsync<T>(string tableName, string filter) where T : TableEntityBase;
        Task<T?> GetRecordAsync<T>(string tableName, string partitionKey, string rowKey) where T : TableEntityBase;
        Task<bool> DeleteRecordAsync(string tableName, string partitionKey, string rowKey);
    }
}
