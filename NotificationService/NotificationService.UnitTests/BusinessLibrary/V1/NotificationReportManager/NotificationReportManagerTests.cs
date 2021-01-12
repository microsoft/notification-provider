﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.UnitTests.BusinesLibrary.V1.NotificationReportManager
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using Microsoft.Azure.Cosmos.Table;
    using Microsoft.Extensions.Configuration;
    using Moq;
    using NotificationService.BusinessLibrary;
    using NotificationService.Common.Logger;
    using NotificationService.Contracts;
    using NotificationService.Contracts.Entities;
    using NotificationService.Data;
    using NotificationService.Data.Interfaces;
    using NUnit.Framework;

    /// <summary>
    /// Tests for NotificationReportManager.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class NotificationReportManagerTests
    {
        private Mock<IRepositoryFactory> EmailNotificationRepositoryFactory { get; set; }

        private ILogger Logger { get; set; }

        private IConfiguration Configuration { get; set; }

        private NotificationReportManager NotificationReportManager { get; set; }

        private Mock<IEmailNotificationRepository> EmailNotificationRepository { get; set; }

        private Mock<IEmailManager> EmailManager { get; set; }

        private Mock<IMailTemplateRepository> MailTemplateRepository { get; set; }

        /// <summary>
        /// Initialization for the tests.
        /// </summary>
        [SetUp]
        public void Setup()
        {
            this.EmailNotificationRepositoryFactory = new Mock<IRepositoryFactory>();
            this.Logger = new Mock<ILogger>().Object;
            this.EmailNotificationRepository = new Mock<IEmailNotificationRepository>();
            this.EmailManager = new Mock<IEmailManager>();
            this.MailTemplateRepository = new Mock<IMailTemplateRepository>();
        }

        /// <summary>
        /// Tests for GetReportNotifications method for valid inputs.
        /// </summary>
        [Test]
        public void GetReportNotificationsWithApplicationFilter()
        {
            Dictionary<string, string> testConfigValues = new Dictionary<string, string>()
            {
                { "RetrySetting:MaxRetries", "10" },
                { "RetrySetting:TransientRetryCount", "3" },
                { NotificationService.Common.Constants.StorageType, StorageType.StorageAccount.ToString() },
            };

            this.Configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(testConfigValues)
                .Build();

            List<NotificationReportResponse> results = new List<NotificationReportResponse>();
            results.Add(new NotificationReportResponse() { NotificationId = "1", Application = "SelectedApp" });
            results.Add(new NotificationReportResponse() { NotificationId = "2", Application = "SelectedApp" });

            var request = new NotificationReportRequest()
            {
                NotificationStatusFilter = new List<NotificationItemStatus> { NotificationItemStatus.Sent, NotificationItemStatus.Processing },
                NotificationPriorityFilter = new List<NotificationPriority> { NotificationPriority.High },
                NotificationTypeFilter = new List<NotificationType> { NotificationType.Mail },
                NotificationIdsFilter = new List<string> { "1" },
                MailSensitivityFilter = new List<MailSensitivity> { MailSensitivity.Normal },
                AccountsUsedFilter = new List<string> { "gtauser" },
                ApplicationFilter = new List<string>() { "test", "SelectedApp", },
                CreatedDateTimeStart = "2020-07-21",
            };

            List<EmailNotificationItemEntity> dbEntities = new List<EmailNotificationItemEntity>();
            dbEntities.Add(new EmailNotificationItemEntity() { NotificationId = "1", Application = "SelectedApp" });
            dbEntities.Add(new EmailNotificationItemEntity() { NotificationId = "2", Application = "SelectedApp" });

            var tuple = Tuple.Create<IList<EmailNotificationItemEntity>, TableContinuationToken>(dbEntities, null);

            _ = this.EmailNotificationRepository
                .Setup(repository => repository.GetEmailNotifications(request))
                .ReturnsAsync(tuple);

            _ = this.EmailNotificationRepositoryFactory
                .Setup(repository => repository.GetRepository(StorageType.StorageAccount))
                .Returns(this.EmailNotificationRepository.Object);

            this.NotificationReportManager = new NotificationReportManager(this.Logger, this.EmailNotificationRepositoryFactory.Object, this.Configuration, this.EmailManager.Object, this.MailTemplateRepository.Object);

            var managerResult = this.NotificationReportManager.GetReportNotifications(request);
            Assert.AreEqual(managerResult.Status.ToString(), "RanToCompletion");
            CollectionAssert.AreEquivalent(managerResult.Result.Item1.Select(x => x.NotificationId), results.Select(x => x.NotificationId));

            this.EmailNotificationRepositoryFactory.Verify(repo => repo.GetRepository(StorageType.StorageAccount).GetEmailNotifications(request));
        }

        /// <summary>
        /// Tests for GetAllTemplateEntities method for valid inputs.
        /// </summary>
        [Test]
        public void GetAllTemplateEntitiesTest()
        {
            Dictionary<string, string> testConfigValues = new Dictionary<string, string>()
            {
                { "RetrySetting:MaxRetries", "10" },
                { "RetrySetting:TransientRetryCount", "3" },
                { NotificationService.Common.Constants.StorageType, StorageType.StorageAccount.ToString() },
            };

            this.Configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(testConfigValues)
                .Build();

            IList<MailTemplateEntity> mailTemplateEntities = new List<MailTemplateEntity>();
            mailTemplateEntities.Add(new MailTemplateEntity()
            {
                TemplateName = "TestTemplate",
                Description = "Test template",
                Content = "Testing the template",
                TemplateType = "Text",
                Application = "TestApp",
            });
            List<MailTemplate> results = new List<MailTemplate>();
            results.Add(new MailTemplate() { TemplateName = "TestTemplate" });

            string applicationName = "TestApp";

            _ = this.MailTemplateRepository
                .Setup(repository => repository.GetAllTemplateEntities(applicationName))
                .ReturnsAsync(mailTemplateEntities);

            this.NotificationReportManager = new NotificationReportManager(this.Logger, this.EmailNotificationRepositoryFactory.Object, this.Configuration, this.EmailManager.Object, this.MailTemplateRepository.Object);

            var managerResult = this.NotificationReportManager.GetAllTemplateEntities(applicationName);
            Assert.AreEqual(managerResult.Status.ToString(), "RanToCompletion");
            CollectionAssert.AreEquivalent(managerResult.Result.Select(x => x.TemplateName), results.Select(x => x.TemplateName));

            this.MailTemplateRepository.Verify(repo => repo.GetAllTemplateEntities(applicationName));
        }

        /// <summary>
        /// Tests for GetNotificationMessageTest method for valid inputs.
        /// </summary>
        [Test]
        public void GetNotificationMessageTest()
        {
            Dictionary<string, string> testConfigValues = new Dictionary<string, string>()
            {
                { "RetrySetting:MaxRetries", "10" },
                { "RetrySetting:TransientRetryCount", "3" },
                { NotificationService.Common.Constants.StorageType, StorageType.StorageAccount.ToString() },
            };

            this.Configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(testConfigValues)
                .Build();

            EmailNotificationItemEntity notificationItemEntity = new EmailNotificationItemEntity()
             {
                Application = "TestApp",
                To = "user@contoso.com",
                Subject = "Test",
                Body = "Test Body",
             };

            MessageBody body = new MessageBody
            {
                Content = "Test Body",
                ContentType = "Text",
            };

            string applicationName = "TestApp";
            string notificationId = Guid.NewGuid().ToString();

            _ = this.EmailNotificationRepository
                .Setup(repository => repository.GetEmailNotificationItemEntity(notificationId))
                .ReturnsAsync(notificationItemEntity);

            _ = this.EmailNotificationRepositoryFactory
                .Setup(repository => repository.GetRepository(StorageType.StorageAccount))
                .Returns(this.EmailNotificationRepository.Object);

            _ = this.EmailManager
               .Setup(mgr => mgr.GetNotificationMessageBodyAsync(applicationName, notificationItemEntity))
               .ReturnsAsync(body);

            this.NotificationReportManager = new NotificationReportManager(this.Logger, this.EmailNotificationRepositoryFactory.Object, this.Configuration, this.EmailManager.Object, this.MailTemplateRepository.Object);

            var managerResult = this.NotificationReportManager.GetNotificationMessage(applicationName, notificationId);
            Assert.AreEqual(managerResult.Status.ToString(), "RanToCompletion");
            Assert.AreEqual(managerResult.Result.Body, body);

            this.EmailNotificationRepositoryFactory.Verify(repo => repo.GetRepository(StorageType.StorageAccount).GetEmailNotificationItemEntity(notificationId));
            this.EmailManager.Verify(mgr => mgr.GetNotificationMessageBodyAsync(applicationName, notificationItemEntity));
        }
    }
}
