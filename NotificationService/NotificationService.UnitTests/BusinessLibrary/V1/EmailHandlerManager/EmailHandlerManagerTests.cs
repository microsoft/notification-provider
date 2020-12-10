namespace NotificationService.UnitTests.BusinessLibrary.V1.EmailHandlerManagerTests
{
    using Moq;
    using NotificationService.BusinessLibrary.Business.v1;
    using NUnit.Framework;
    using NotificationService.Common.Logger;
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Microsoft.Extensions.Configuration;
    using NotificationService.Common;
    using Microsoft.Extensions.Options;
    using NotificationService.Data;
    using NotificationService.BusinessLibrary;
    using NotificationService.Contracts.Models;
    using System.Threading.Tasks;
    using NotificationService.Contracts.Entities;
    using NotificationService.Contracts;
    using System.Linq;

    public class EmailHandlerManagerTests
    {
        /// <summary>
        /// Gets or sets Logger.
        /// </summary>
        public ILogger Logger { get; set; }

        /// <summary>
        /// Gets or sets Configuration.
        /// </summary>
        public Mock<IConfiguration> Configuration { get; set; }

        /// <summary>
        /// The ms graph settings
        /// </summary>
        private IOptions<MSGraphSetting> msGraphSettings;

        private Mock<ICloudStorageClient> mockedCloudStorageClient;

        private Mock<IEmailManager> mockedEmailManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="EmailHandlerManagerTests"/> class.
        /// </summary>
        public EmailHandlerManagerTests()
        {
            this.Configuration = new Mock<IConfiguration>();
            this.Logger = new Mock<ILogger>().Object;
            this.msGraphSettings = Options.Create<MSGraphSetting>(new MSGraphSetting { Authority = "aa" });
            this.mockedCloudStorageClient = new Mock<ICloudStorageClient>();
            this.mockedEmailManager = new Mock<IEmailManager>();

        }

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
            var emailHandler = new EmailHandlerManager(this.Configuration.Object, this.msGraphSettings, this.mockedCloudStorageClient.Object, this.Logger, this.mockedEmailManager.Object);
            var meetingNotificationItems = new List<MeetingNotificationItem> { new MeetingNotificationItem { RequiredAttendees = "user@contoso.com" } };
            var response = await emailHandler.QueueMeetingNotifications("TestApp", meetingNotificationItems.ToArray());
            Assert.IsNotNull(response);
            Assert.IsTrue(response.Count == 1);
            Assert.IsTrue(response[0].NotificationId == "notificationId1");

        }
    }
}
