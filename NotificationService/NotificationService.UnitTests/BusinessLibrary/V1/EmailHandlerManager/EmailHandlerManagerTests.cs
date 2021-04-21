// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.UnitTests.BusinessLibrary.V1.EmailHandlerManagerTests
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Options;
    using Moq;
    using Newtonsoft.Json;
    using NotificationService.BusinessLibrary;
    using NotificationService.BusinessLibrary.Business.V1;
    using NotificationService.Common;
    using NotificationService.Common.Configurations;
    using NotificationService.Common.Logger;
    using NotificationService.Contracts;
    using NotificationService.Contracts.Entities;
    using NotificationService.Contracts.Models;
    using NotificationService.Data;
    using NUnit.Framework;

    /// <summary>
    /// EmailHandlerManagerTests.
    /// </summary>
    public class EmailHandlerManagerTests
    {
        /// <summary>
        /// The ms graph settings.
        /// </summary>
        private readonly IOptions<MSGraphSetting> msGraphSettings;

        private readonly Mock<ICloudStorageClient> mockedCloudStorageClient;

        private readonly Mock<IEmailManager> mockedEmailManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="EmailHandlerManagerTests"/> class.
        /// </summary>
        public EmailHandlerManagerTests()
        {
            var config = new Dictionary<string, string>()
            {
                { ConfigConstants.AllowedMaxResendDurationInDays, "1" },
                { ConfigConstants.StorageAccountConfigSectionKey, JsonConvert.SerializeObject(new StorageAccountSetting() { NotificationQueueName = "notifications-queue", }) },
            };

            this.Configuration = new ConfigurationBuilder().AddInMemoryCollection(config).Build();
            this.Logger = new Mock<ILogger>().Object;
            this.msGraphSettings = Options.Create<MSGraphSetting>(new MSGraphSetting { Authority = "aa" });
            this.mockedCloudStorageClient = new Mock<ICloudStorageClient>();
            this.mockedEmailManager = new Mock<IEmailManager>();
        }

        /// <summary>
        /// Gets or sets Logger.
        /// </summary>
        public ILogger Logger { get; set; }

        /// <summary>
        /// Gets or sets Configuration.
        /// </summary>
        public IConfiguration Configuration { get; set; }

        /// <summary>
        /// Queues the meeting notifications tests.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Test]
        public async Task QueueMeetingNotificationsTests()
        {
            var items = new List<MeetingNotificationItemEntity> { new MeetingNotificationItemEntity { Application = "TestApp", RequiredAttendees = "user@contoso.com" } };
            _ = this.mockedEmailManager.Setup(x => x.CreateMeetingNotificationEntities("TestApp", It.IsAny<MeetingNotificationItem[]>(), NotificationService.Contracts.NotificationItemStatus.Queued)).ReturnsAsync(items);
            _ = this.mockedEmailManager.Setup(x => x.NotificationEntitiesToResponse(new List<NotificationResponse>(), It.Is<List<MeetingNotificationItemEntity>>(y => y.Any(c => c.Application == "TestApp")))).Returns(new List<NotificationResponse> { new NotificationResponse { NotificationId = "notificationId1" } });
            var emailHandler = new EmailHandlerManager(this.Configuration, this.msGraphSettings, this.mockedCloudStorageClient.Object, this.Logger, this.mockedEmailManager.Object);
            var meetingNotificationItems = new List<MeetingNotificationItem> { new MeetingNotificationItem { RequiredAttendees = "user@contoso.com" } };
            var response = await emailHandler.QueueMeetingNotifications("TestApp", meetingNotificationItems.ToArray());
            Assert.IsNotNull(response);
            Assert.IsTrue(response.Count == 1);
            Assert.IsTrue(response[0].NotificationId == "notificationId1");
        }
    }
}
