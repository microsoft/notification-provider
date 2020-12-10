// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.UnitTests.Controllers.V1.EmailController
{
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;
    using Moq;
    using NotificationHandler.Controllers;
    using NotificationService.BusinessLibrary;
    using NotificationService.BusinessLibrary.Interfaces;
    using NotificationService.Common.Logger;
    using NUnit.Framework;

    /// <summary>
    /// Tests for Email Template method of Email Controller.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class TemplateAPITests
    {
        private readonly string applicationName = "TestApp";
        private readonly string templateName = "TestTemplate";

        private Mock<IEmailHandlerManager> emailHandlerManager;
        private Mock<IMailTemplateManager> mailTemplateManager;
        private ILogger logger;

        /// <summary>
        /// Initialization for the tests.
        /// </summary>
        [SetUp]
        public void Setup()
        {
            this.emailHandlerManager = new Mock<IEmailHandlerManager>();
            this.mailTemplateManager = new Mock<IMailTemplateManager>();
            this.logger = new Mock<ILogger>().Object;
        }

        /// <summary>
        /// Tests for DeleteMailTemplate method.
        /// </summary>
        [Test]
        public void DeleteMailTemplateTest()
        {
            EmailController emailController = new EmailController(this.emailHandlerManager.Object, this.mailTemplateManager.Object, this.logger);
            bool response = true;

            _ = this.mailTemplateManager
                .Setup(mailTemplateManager => mailTemplateManager.DeleteMailTemplate(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.FromResult(response));

            var result = emailController.DeleteMailTemplate(this.applicationName, this.templateName);
            Assert.AreEqual(result.Status.ToString(), "RanToCompletion");
            this.mailTemplateManager.Verify(mgr => mgr.DeleteMailTemplate(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            Assert.Pass();
        }
    }
}
