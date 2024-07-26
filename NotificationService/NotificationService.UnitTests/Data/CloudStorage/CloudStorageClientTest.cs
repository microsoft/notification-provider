namespace NotificationService.UnitTests.Data.CloudStorage
{
    using Azure;
    using Azure.Storage.Blobs;
    using Azure.Storage.Blobs.Models;
    using Moq;
    using NotificationService.Common.Encryption;
    using NotificationService.Common;
    using NotificationService.Contracts;
    using NotificationService.Data;
    using NotificationService.Data.Repositories;
    using NUnit.Framework;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    public class CloudStorageClientTest : CloudStorageClientSetup
    {        
        public Mock<IEncryptionService> EncryptionService { get; set; }

        /// <summary>
        /// Tests for GetBlobClientBlobServiceClientRefTest method for valid inputs.
        /// </summary>
        [Test]
        public void GetBlobClientBlobServiceClientRefTest()
        {
            var test = SUT.GetBlobServiceClient(this.storageAccountSetting.Value);
            Assert.IsNotNull(test.AccountName);
        }
    }
}
