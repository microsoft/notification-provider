// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// The Root Ping Controller.
    /// </summary>
    public class RootPingController : Controller
    {
        /// <summary>
        /// The instance for <see cref="ILogger{RootPingController}"/>.
        /// </summary>
        private readonly ILogger<RootPingController> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="RootPingController"/> class.
        /// Constructor for Root Ping Controller.
        /// </summary>
        /// <param name="logger">The instance for <see cref="ILogger{RootPingController}"/>.</param>
        public RootPingController(ILogger<RootPingController> logger)
        {
            this.logger = logger;
        }

        /// <summary>
        /// Generates a success response as a ping.
        /// </summary>
        /// <returns> The instance for <see cref="IActionResult"/>. </returns>
        [HttpGet("")]
        public IActionResult Ping()
        {
            this.logger.LogInformation($"Invoked {nameof(this.Ping)} method in {nameof(RootPingController)}.");
            return this.Ok();
        }
    }
}