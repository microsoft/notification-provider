namespace NotificationHandler.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using NotificationService.BusinessLibrary.Interfaces;
    using NotificationService.Common;

    /// <summary>
    /// Controller to handle notification client configs.
    /// </summary>
    [Route("v1/client")]
    public class NotificationClientController : Controller
    {
        /// <summary>
        /// Instance of <see cref="notificationClientManager"/>.
        /// </summary>
        private readonly INotificationClientManager notificationClientManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationClientController"/> class.
        /// </summary>
        /// <param name="notificationClientManager">An instance of <see cref="notificationClientManager"/>.</param>
        public NotificationClientController(INotificationClientManager notificationClientManager)
        {
            this.notificationClientManager = notificationClientManager;
        }

        /// <summary>
        /// API to get Applications.
        /// </summary>
        /// <returns>Returns list of applications.</returns>
        [HttpGet]
        [Authorize(AuthenticationSchemes = ApplicationConstants.BearerAuthenticationScheme)]
        [Route("applications")]
        public IActionResult GetApplications()
        {
            var result = this.notificationClientManager.GetApplications();
            return this.Accepted(result);
        }

    }
}
