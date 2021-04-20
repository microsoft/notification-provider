// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.UnitTests.BusinessLibrary.Providers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using DirectSend;
    using DirectSend.Models.Mail;
    using Microsoft.Extensions.Configuration;
    using Moq;
    using Newtonsoft.Json;
    using NotificationService.BusinessLibrary;
    using NotificationService.BusinessLibrary.Providers;
    using NotificationService.Common.Configurations;
    using NotificationService.Common.Logger;
    using NotificationService.Contracts.Entities;
    using NUnit.Framework;

    /// <summary>
    /// DirectSendNotificationProvider Unit Test class.
    /// </summary>
    public class DirectSendNotificationProviderTests
    {
        /// <summary>
        /// The mocked email service.
        /// </summary>
        private readonly Mock<IEmailService> mockedEmailService;

        /// <summary>
        /// The mocked email manager.
        /// </summary>
        private readonly Mock<IEmailManager> mockedEmailManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectSendNotificationProviderTests"/> class.
        /// </summary>
        public DirectSendNotificationProviderTests()
        {
            this.Configuration = new Mock<IConfiguration>();
            this.mockedEmailService = new Mock<IEmailService>();
            this.mockedEmailManager = new Mock<IEmailManager>();
            this.Logger = new Mock<ILogger>().Object;
            _ = this.Configuration.Setup(x => x["MailSettings"]).Returns(JsonConvert.SerializeObject(new List<MailSettings> { new MailSettings { ApplicationName = "TestApp", SendForReal = false, ToOverride = "dummy@contoso.com" } }));
            _ = this.Configuration.Setup(x => x["DirectSendSetting__SmtpServer"]).Returns("testServer");
            _ = this.Configuration.Setup(x => x["DirectSendSetting__SmtpPort"]).Returns("25");
            var configurationSection = new Mock<IConfigurationSection>();
            _ = configurationSection.Setup(a => a.Value).Returns(JsonConvert.SerializeObject(new DirectSendSetting { SmtpServer = "testServer", SmtpPort = "25" }));
            _ = this.Configuration.Setup(x => x.GetSection("DirectSendSetting")).Returns(configurationSection.Object);
            _ = this.mockedEmailManager.Setup(x => x.GetMeetingInviteBodyAsync(It.IsAny<string>(), It.IsAny<MeetingNotificationItemEntity>())).ReturnsAsync(new NotificationService.Contracts.MessageBody { Content = "Test" });
        }

        /// <summary>
        /// Gets or sets Logger.
        /// </summary>
        public ILogger Logger { get; set; }

        /// <summary>
        /// Gets or sets Configuration.
        /// </summary>
        public Mock<IConfiguration> Configuration { get; set; }

        [Test]
        public async Task ProcessMeetingNotificationEntitiesTests()
        {
            var provider = new DirectSendNotificationProvider(this.Configuration.Object, this.mockedEmailService.Object, this.Logger, this.mockedEmailManager.Object);
            var entities = new List<MeetingNotificationItemEntity> { new MeetingNotificationItemEntity { NotificationId = "notificationId1", RequiredAttendees = "user@contos.com;user1@contos.com", OptionalAttendees = "user2@contos.com" } };
            await provider.ProcessMeetingNotificationEntities("TestApp", entities);
            this.mockedEmailService.Verify(x => x.SendMeetingInviteAsync(It.Is<EmailMessage>(q => q.ToAddresses.Any(r => r.Address == "dummy@contoso.com") && q.CcAddresses == null)), Times.Once);
            _ = this.Configuration.Setup(x => x["MailSettings"]).Returns(JsonConvert.SerializeObject(new List<MailSettings> { new MailSettings { ApplicationName = "TestApp", SendForReal = true, ToOverride = "dummy@contoso.com" } }));
            provider = new DirectSendNotificationProvider(this.Configuration.Object, this.mockedEmailService.Object, this.Logger, this.mockedEmailManager.Object);
            await provider.ProcessMeetingNotificationEntities("TestApp", entities);
            this.mockedEmailService.Verify(x => x.SendMeetingInviteAsync(It.Is<EmailMessage>(q => q.ToAddresses.Any(r => r.Address == "user@contos.com") && q.ToAddresses.Any(r => r.Address == "user2@contos.com"))), Times.Once);
        }

        [Test]
        public async Task ProcessMeetingNotificationEntitiesTests_2()
        {
            var provider = new DirectSendNotificationProvider(this.Configuration.Object, this.mockedEmailService.Object, this.Logger, this.mockedEmailManager.Object);
            var entities = new List<MeetingNotificationItemEntity> { new MeetingNotificationItemEntity { NotificationId = "notificationId1", RequiredAttendees = "user@contos.com;user1@contos.com", OptionalAttendees = "user2@contos.com", RecurrencePattern = NotificationService.Contracts.MeetingRecurrencePattern.Daily, Interval = 2 } };
            _ = this.Configuration.Setup(x => x["MailSettings"]).Returns(JsonConvert.SerializeObject(new List<MailSettings> { new MailSettings { ApplicationName = "TestApp", SendForReal = true, ToOverride = "dummy@contoso.com" } }));
            provider = new DirectSendNotificationProvider(this.Configuration.Object, this.mockedEmailService.Object, this.Logger, this.mockedEmailManager.Object);
            await provider.ProcessMeetingNotificationEntities("TestApp", entities);
            this.mockedEmailService.Verify(x => x.SendMeetingInviteAsync(It.Is<EmailMessage>(q => q.ToAddresses.Any(r => r.Address == "user@contos.com") && q.ToAddresses.Any(r => r.Address == "user2@contos.com") && q.Content.Contains("RRULE:FREQ=DAILY;INTERVAL=2;COUNT=1"))), Times.Once);
            entities = new List<MeetingNotificationItemEntity> { new MeetingNotificationItemEntity { NotificationId = "notificationId1", RequiredAttendees = "user@contos.com;user1@contos.com", OptionalAttendees = "user2@contos.com", RecurrencePattern = NotificationService.Contracts.MeetingRecurrencePattern.Weekly, Interval = 2, DaysOfWeek = "3,5" } };
            await provider.ProcessMeetingNotificationEntities("TestApp", entities);
            this.mockedEmailService.Verify(x => x.SendMeetingInviteAsync(It.Is<EmailMessage>(q => q.ToAddresses.Any(r => r.Address == "user@contos.com") && q.ToAddresses.Any(r => r.Address == "user2@contos.com") && q.Content.Contains("RRULE:FREQ=WEEKLY;BYDAY=WE,FR;INTERVAL=2;COUNT=1"))), Times.Once);
            entities = new List<MeetingNotificationItemEntity> { new MeetingNotificationItemEntity { NotificationId = "notificationId1", RequiredAttendees = "user@contos.com;user1@contos.com", OptionalAttendees = "user2@contos.com", RecurrencePattern = NotificationService.Contracts.MeetingRecurrencePattern.Monthly, Interval = 3, DaysOfWeek = "3,5", DayOfWeekByMonth = "4" } };
            await provider.ProcessMeetingNotificationEntities("TestApp", entities);
            this.mockedEmailService.Verify(x => x.SendMeetingInviteAsync(It.Is<EmailMessage>(q => q.ToAddresses.Any(r => r.Address == "user@contos.com") && q.ToAddresses.Any(r => r.Address == "user2@contos.com") && q.Content.Contains("RRULE:FREQ=MONTHLY;BYSETPOS=4;BYDAY=WE,FR;INTERVAL=3;COUNT=1"))), Times.Once);
            entities = new List<MeetingNotificationItemEntity> { new MeetingNotificationItemEntity { NotificationId = "notificationId1", RequiredAttendees = "user@contos.com;user1@contos.com", OptionalAttendees = "user2@contos.com", RecurrencePattern = NotificationService.Contracts.MeetingRecurrencePattern.Yearly, Interval = 2, DaysOfWeek = "3,5", DayOfWeekByMonth = "4", SequenceNumber = 1, IsPrivate = true, ReminderMinutesBeforeStart = "15" } };
            await provider.ProcessMeetingNotificationEntities("TestApp", entities);
            this.mockedEmailService.Verify(x => x.SendMeetingInviteAsync(It.Is<EmailMessage>(q => q.ToAddresses.Any(r => r.Address == "user@contos.com") && q.ToAddresses.Any(r => r.Address == "user2@contos.com") && q.Content.Contains("RRULE:FREQ=YEARLY;BYDAY=WE,FR;BYSETPOS=4;BYMONTH=0;COUNT=1") && q.Content.Contains("TRIGGER:-P15M"))), Times.Once);
        }
    }
}
