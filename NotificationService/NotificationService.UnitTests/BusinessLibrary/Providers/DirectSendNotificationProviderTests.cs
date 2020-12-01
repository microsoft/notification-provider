namespace NotificationService.UnitTests.BusinessLibrary.Providers
{
    using Microsoft.Extensions.Configuration;
    using Moq;
    using NotificationService.BusinessLibrary;
    using NotificationService.Data;
    using NUnit.Framework;
    using NotificationService.Common.Logger;
    using DirectSend;
    using NotificationService.BusinessLibrary.Providers;
    using System.Threading;
    using NotificationService.Contracts.Entities;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using NotificationService.Common.Configurations;
    using Newtonsoft.Json;
    using DirectSend.Models.Mail;
    using System.Linq;

    public class DirectSendNotificationProviderTests
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
        /// The mocked email service.
        /// </summary>
        private Mock<IEmailService> mockedEmailService;

        /// <summary>
        /// The mocked email manager.
        /// </summary>
        private Mock<IEmailManager> mockedEmailManager;

        public DirectSendNotificationProviderTests()
        {
            this.Configuration = new Mock<IConfiguration>();
            this.mockedEmailService = new Mock<IEmailService>();
            this.mockedEmailManager = new Mock<IEmailManager>();
            this.Logger = new Mock<ILogger>().Object;
            _ = this.Configuration.Setup(x => x["MailSettings"]).Returns(JsonConvert.SerializeObject(new List<MailSettings> { new MailSettings { ApplicationName= "TestApp", SendForReal = false, ToOverride = "dummy@contoso.com" } }));
            _ = this.Configuration.Setup(x => x["DirectSendSetting__SmtpServer"]).Returns("testServer");
            _ = this.Configuration.Setup(x => x["DirectSendSetting__SmtpPort"]).Returns("25");
            var configurationSection = new Mock<IConfigurationSection>();
            _ = configurationSection.Setup(a => a.Value).Returns(JsonConvert.SerializeObject(new DirectSendSetting { SmtpServer = "testServer", SmtpPort = "25" }));
            _ = this.Configuration.Setup(x => x.GetSection("DirectSendSetting")).Returns(configurationSection.Object);
        }

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
    }
}
