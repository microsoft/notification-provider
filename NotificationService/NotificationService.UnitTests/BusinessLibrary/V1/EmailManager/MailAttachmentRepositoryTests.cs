// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.UnitTests.BusinessLibrary.V1.EmailManager
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Moq;
    using Newtonsoft.Json;
    using NotificationService.Common;
    using NotificationService.Common.Encryption;
    using NotificationService.Common.Logger;
    using NotificationService.Contracts;
    using NotificationService.Contracts.Entities;
    using NotificationService.Data;
    using NotificationService.Data.Repositories;
    using NUnit.Framework;

    /// <summary>
    /// MailAttachmentRepositoryTests.
    /// </summary>
    public class MailAttachmentRepositoryTests
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MailAttachmentRepositoryTests"/> class.
        /// </summary>
        public MailAttachmentRepositoryTests()
        {
            this.Logger = new Mock<ILogger>().Object;
            this.MockedCloudStorageClient = new Mock<ICloudStorageClient>();
        }

        /// <summary>
        /// Gets or sets Logger.
        /// </summary>
        public ILogger Logger { get; set; }

        /// <summary>
        /// Gets or sets Configuration.
        /// </summary>
        public Mock<ICloudStorageClient> MockedCloudStorageClient { get; set; }

        /// <summary>
        /// Gets or sets Encryption Service Mock.
        /// </summary>
        public Mock<IEncryptionService> EncryptionService { get; set; }

        /// <summary>
        /// UploadEmailTests.
        /// </summary>
        /// <returns>A<see cref="Task"/>.</returns>
        [Test]

        public async Task UploadEmailTests()
        {
            this.EncryptionService = new Mock<IEncryptionService>();
            var notificationId = Guid.NewGuid().ToString();
            var notifications = new List<EmailNotificationItemEntity> { new EmailNotificationItemEntity { NotificationId = notificationId } };
            _ = this.MockedCloudStorageClient.Setup(x => x.UploadBlobAsync($"TestApp/{ApplicationConstants.EmailNotificationsFolderName}/{notificationId}", "TestString")).ReturnsAsync($"TestApp/{ApplicationConstants.EmailNotificationsFolderName}/{notificationId}");
            _ = this.EncryptionService.Setup(x => x.Encrypt(It.IsAny<string>())).Returns("TestString");
            var repo = new MailAttachmentRepository(this.Logger, this.MockedCloudStorageClient.Object, this.EncryptionService.Object);
            var updatednotifications = await repo.UploadEmail(notifications, NotificationType.Mail.ToString(), "TestApp").ConfigureAwait(false);
            Assert.IsTrue(updatednotifications.Count == 1);
            this.MockedCloudStorageClient.Verify(x => x.UploadBlobAsync($"TestApp/{ApplicationConstants.EmailNotificationsFolderName}/{notificationId}", "TestString"), Times.Once);
            this.EncryptionService.Verify(x => x.Encrypt(It.IsAny<string>()), Times.Once);
        }

        /// <summary>
        /// UploadEmailTests.
        /// </summary>
        /// <returns>A<see cref="Task"/>.</returns>
        [Test]

        public async Task UploadMeetingInviteTests()
        {
            this.EncryptionService = new Mock<IEncryptionService>();
            var notificationId = Guid.NewGuid().ToString();
            var notifications = new List<MeetingNotificationItemEntity> { new MeetingNotificationItemEntity { NotificationId = notificationId } };
            _ = this.MockedCloudStorageClient.Setup(x => x.UploadBlobAsync($"TestApp/{ApplicationConstants.MeetingNotificationsFolderName}/{notificationId}", "TestString")).ReturnsAsync($"TestApp/{ApplicationConstants.EmailNotificationsFolderName}/{notificationId}");
            _ = this.EncryptionService.Setup(x => x.Encrypt(It.IsAny<string>())).Returns("TestString");
            var repo = new MailAttachmentRepository(this.Logger, this.MockedCloudStorageClient.Object, this.EncryptionService.Object);
            var updatednotifications = await repo.UploadMeetingInvite(notifications, "TestApp").ConfigureAwait(false);
            Assert.IsTrue(updatednotifications.Count == 1);
            this.MockedCloudStorageClient.Verify(x => x.UploadBlobAsync($"TestApp/{ApplicationConstants.MeetingNotificationsFolderName}/{notificationId}", "TestString"), Times.Once);
            this.EncryptionService.Verify(x => x.Encrypt(It.IsAny<string>()), Times.Once);
        }

        /// <summary>
        /// DownloadEmailTests.
        /// </summary>
        /// <returns>A Task.</returns>
        [Test]
        public async Task DownloadEmailTests()
        {
            this.EncryptionService = new Mock<IEncryptionService>();
            var notificationId = Guid.NewGuid().ToString();
            var notifications = new List<EmailNotificationItemEntity> { new EmailNotificationItemEntity { NotificationId = notificationId } };
            var blobEmailData = new BlobEmailData
            {
                NotificationId = notificationId,
                Body = "Test Body",
                Attachments = new List<NotificationAttachmentEntity> { new NotificationAttachmentEntity { FileBase64 = "VEhpcyBpcyBhIHRlc3QgYXR0YWNobWVudCBmaWxlLg==", FileName = "Test.txt", IsInline = true } },
            };

            _ = this.MockedCloudStorageClient.Setup(x => x.DownloadBlobAsync($"TestApp/{ApplicationConstants.EmailNotificationsFolderName}/{notificationId}")).ReturnsAsync("Test Encrypted String");
            _ = this.EncryptionService.Setup(x => x.Decrypt(It.Is<string>(x => x == "Test Encrypted String"))).Returns(JsonConvert.SerializeObject(blobEmailData));
            var repo = new MailAttachmentRepository(this.Logger, this.MockedCloudStorageClient.Object, this.EncryptionService.Object);
            var updatednotifications = await repo.DownloadEmail(notifications, "TestApp").ConfigureAwait(false);
            Assert.IsTrue(updatednotifications.Count == 1);
            Assert.IsTrue(updatednotifications.First().Body.Equals("Test Body"));
            Assert.IsTrue(updatednotifications.First().Attachments.Any(x => x.FileBase64.Equals("VEhpcyBpcyBhIHRlc3QgYXR0YWNobWVudCBmaWxlLg==")));
            Assert.IsTrue(updatednotifications.First().Attachments.Any(x => x.FileName.Equals("Test.txt")));
            Assert.IsTrue(updatednotifications.First().Attachments.Any(x => x.IsInline));
            this.MockedCloudStorageClient.Verify(x => x.DownloadBlobAsync($"TestApp/{ApplicationConstants.EmailNotificationsFolderName}/{notificationId}"), Times.Once);
            this.EncryptionService.Verify(x => x.Decrypt(It.Is<string>(x => x == "Test Encrypted String")), Times.Once);
        }

        /// <summary>
        /// DownloadMeetingTests.
        /// </summary>
        /// <returns>A Task.</returns>
        [Test]
        public async Task DownloadMeetingTests()
        {
            this.EncryptionService = new Mock<IEncryptionService>();
            var notificationId = Guid.NewGuid().ToString();
            var notifications = new List<MeetingNotificationItemEntity> { new MeetingNotificationItemEntity { NotificationId = notificationId } };
            var blobEmailData = new BlobEmailData
            {
                NotificationId = notificationId,
                Body = "Test Body",
                Attachments = new List<NotificationAttachmentEntity> { new NotificationAttachmentEntity { FileBase64 = "VEhpcyBpcyBhIHRlc3QgYXR0YWNobWVudCBmaWxlLg==", FileName = "Test.txt", IsInline = true } },
            };

            _ = this.MockedCloudStorageClient.Setup(x => x.DownloadBlobAsync($"TestApp/{ApplicationConstants.MeetingNotificationsFolderName}/{notificationId}")).ReturnsAsync("Test Encrypted String");
            _ = this.EncryptionService.Setup(x => x.Decrypt(It.Is<string>(x => x == "Test Encrypted String"))).Returns(JsonConvert.SerializeObject(blobEmailData));
            var repo = new MailAttachmentRepository(this.Logger, this.MockedCloudStorageClient.Object, this.EncryptionService.Object);
            var updatednotifications = await repo.DownloadMeetingInvite(notifications, "TestApp").ConfigureAwait(false);
            Assert.IsTrue(updatednotifications.Count == 1);
            Assert.IsTrue(updatednotifications.First().Body.Equals("Test Body"));
            Assert.IsTrue(updatednotifications.First().Attachments.Any(x => x.FileBase64.Equals("VEhpcyBpcyBhIHRlc3QgYXR0YWNobWVudCBmaWxlLg==")));
            Assert.IsTrue(updatednotifications.First().Attachments.Any(x => x.FileName.Equals("Test.txt")));
            Assert.IsTrue(updatednotifications.First().Attachments.Any(x => x.IsInline));
            this.MockedCloudStorageClient.Verify(x => x.DownloadBlobAsync($"TestApp/{ApplicationConstants.MeetingNotificationsFolderName}/{notificationId}"), Times.Once);
            this.EncryptionService.Verify(x => x.Decrypt(It.Is<string>(x => x == "Test Encrypted String")), Times.Once);
        }
    }
}
