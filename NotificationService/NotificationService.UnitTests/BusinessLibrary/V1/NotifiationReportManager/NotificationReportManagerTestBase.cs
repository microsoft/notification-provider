// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.UnitTests.BusinessLibrary.V1.NotifiationReportManager
{
    using Microsoft.Extensions.Configuration;
    using Moq;
    using NotificationService.BusinessLibrary;
    using NotificationService.BusinessLibrary.Interfaces;
    using NotificationService.Common.Logger;
    using NotificationService.Data;
    using NotificationService.Data.Interfaces;

    /// <summary>
    /// Notification Report Manager Test base class.
    /// </summary>
    public class NotificationReportManagerTestBase
    {
        /// <summary>
        /// Mocked object of ILogger.
        /// </summary>
#pragma warning disable SA1401 // Fields should be private
        protected Mock<ILogger> logger;
#pragma warning restore SA1401 // Fields should be private

        /// <summary>
        /// Mocked object of IRepositoryFactory.
        /// </summary>
#pragma warning disable SA1401 // Fields should be private
        protected Mock<IRepositoryFactory> repositoryFactory;
#pragma warning restore SA1401 // Fields should be private

        /// <summary>
        /// Mocked object of IConfiguration.
        /// </summary>
#pragma warning disable SA1401 // Fields should be private
        protected IConfiguration configuration;
#pragma warning restore SA1401 // Fields should be private

        /// <summary>
        /// Mocked object of IMailTemplateRepository.
        /// </summary>
#pragma warning disable SA1401 // Fields should be private
        protected Mock<IMailTemplateRepository> mailTemplateRepository;
#pragma warning restore SA1401 // Fields should be private

        /// <summary>
        /// Mocked object of IMailTemplateManager.
        /// </summary>
#pragma warning disable SA1401 // Fields should be private
        protected Mock<IMailTemplateManager> templateManager;
#pragma warning restore SA1401 // Fields should be private

        /// <summary>
        /// Mocked object of ITemplateMerge.
        /// </summary>
#pragma warning disable SA1401 // Fields should be private
        protected Mock<ITemplateMerge> templateMerge;
#pragma warning restore SA1401 // Fields should be private

        /// <summary>
        /// Class Under Test.
        /// </summary>
#pragma warning disable SA1401 // Fields should be private
        protected NotificationReportManager classUnderTest;
#pragma warning restore SA1401 // Fields should be private

        /// <summary>
        /// Gets the name of the application.
        /// </summary>
        /// <value>
        /// The name of the application.
        /// </value>
        protected string ApplicationName { get; } = "Application 1";

        /// <summary>
        /// Setup Base.
        /// </summary>
        public virtual void SetupBase()
        {
            this.logger = new Mock<ILogger>();
            this.repositoryFactory = new Mock<IRepositoryFactory>();
            this.mailTemplateRepository = new Mock<IMailTemplateRepository>();
            this.templateManager = new Mock<IMailTemplateManager>();
            this.templateMerge = new Mock<ITemplateMerge>();
        }
    }
}
