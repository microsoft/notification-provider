// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.BusinessLibrary.Business.v1
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json;
    using NotificationService.BusinessLibrary.Interfaces;
    using NotificationService.Common.Configurations;
    using NotificationService.Common.Logger;
    using NotificationService.Contracts;

    /// <summary>
    /// This class handles the request from notification client.
    /// </summary>
    public class NotificationClientManager : INotificationClientManager
    {

        /// <summary>
        /// Instance of <see cref="ILogger"/>.
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// An Instance of Configuration. 
        /// </summary>
        private readonly IConfiguration configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationClientManager"/> class.
        /// </summary>
        /// <param name="logger">An instance of <see cref="AILogger"/>.</param>
        /// <param name="configuration">Instance of <see cref="IConfiguration"/>.</param>
        public NotificationClientManager(ILogger logger, IConfiguration configuration)
        {
            this.logger = logger;
            this.configuration = configuration;
        }

        /// <inheritdoc/>
        public IList<string> GetApplications()
        {
            this.logger.TraceInformation($"Started {nameof(this.GetApplications)} method of {nameof(NotificationClientManager)}.");
            var applicationAccounts = JsonConvert.DeserializeObject<List<ApplicationAccounts>>(this.configuration?[ConfigConstants.ApplicationAccountsConfigSectionKey]);
            var applications = applicationAccounts.Select(appName => appName.ApplicationName).ToList();
            this.logger.TraceInformation($"Finished {nameof(this.GetApplications)} method of {nameof(NotificationClientManager)}.");
            return applications;
        }
    }
}
