// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.BusinessLibrary.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using NotificationService.Contracts.Entities.Web;

    /// <summary>
    /// The <see cref="INotificationDelivery"/> interface provides mechanism to handle notification delivery status.
    /// </summary>
    public interface INotificationDelivery
    {
        /// <summary>
        /// Marks the notification delivered asynchronously.
        /// </summary>
        /// <param name="notificationIds">The notification identifiers..</param>
        /// <param name="deliveryChannel">The value of <see cref="NotificationDeliveryChannel"/>.</param>
        /// <param name="throwExceptions"><c>true</c> if this method can throw exception;<c>false</c> to log (only) the exceptional conditions if required.</param>
        /// <returns>The instance of <see cref="Task"/> representing an asynchronous operation.</returns>
        Task MarkNotificationDeliveredAsync(IEnumerable<string> notificationIds, NotificationDeliveryChannel deliveryChannel, bool throwExceptions = true);
    }
}
