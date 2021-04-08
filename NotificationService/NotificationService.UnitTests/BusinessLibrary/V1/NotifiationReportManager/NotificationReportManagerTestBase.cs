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
        protected Mock<ILogger> logger;

        /// <summary>
        /// Mocked object of IRepositoryFactory.
        /// </summary>
        protected Mock<IRepositoryFactory> repositoryFactory;

        /// <summary>
        /// Mocked object of IConfiguration.
        /// </summary>
        protected IConfiguration configuration;

        /// <summary>
        /// Mocked object of IMailTemplateRepository.
        /// </summary>
        protected Mock<IMailTemplateRepository> mailTemplateRepository;

        /// <summary>
        /// Mocked object of IMailTemplateManager.
        /// </summary>
        protected Mock<IMailTemplateManager> templateManager;

        /// <summary>
        /// Mocked object of ITemplateMerge.
        /// </summary>
        protected Mock<ITemplateMerge> templateMerge;

        /// <summary>
        /// Class Under Test.
        /// </summary>
        protected NotificationReportManager classUnderTest;

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
