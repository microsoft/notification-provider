// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationHandler.Controllers.V1
{
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Mvc;
    using NotificationService.Common.Logger;

    /// <summary>
    /// Base Controller for Notification Handler service.
    /// </summary>
    public class BaseController : Controller
    {
        /// <summary>
        /// Instance of <see cref="ILogger"/>.
        /// </summary>
#pragma warning disable SA1401 // Fields should be private
#pragma warning disable CA1051 // Do not declare visible instance fields
        protected readonly ILogger logger;
#pragma warning restore CA1051 // Do not declare visible instance fields
#pragma warning restore SA1401 // Fields should be private

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseController"/> class.
        /// </summary>
        /// <param name="logger">An instance of <see cref="ILogger"/>.</param>
        public BaseController(ILogger logger)
        {
            this.logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Logs and rethrow the exception.
        /// </summary>
        /// <param name="message">Error message.</param>
        /// <param name="inputName">Name of input type.</param>
        /// <param name="traceProps">custom properties, add more dimensions to this, so it will be easy to trace and query.</param>
        protected void LogAndThrowArgumentNullException(string message, string inputName, Dictionary<string, string> traceProps)
        {
            var argumentException = new System.ArgumentNullException(inputName, message);
            this.logger.TraceInformation(argumentException.Message, traceProps);
            throw argumentException;
        }
    }
}
