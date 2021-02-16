// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.BusinessLibrary.Business.V1
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using NotificationService.BusinessLibrary.Interfaces;
    using NotificationService.BusinessLibrary.Providers;
    using NotificationService.Contracts.Entities;
    using NotificationService.Contracts.Entities.Web;
    using NotificationService.Contracts.Models.Web.Request;
    using NotificationService.Contracts.Models.Web.Response;
    using NotificationService.Data.Interfaces;

    /// <summary>
    /// The <see cref="NotificationsManager"/> class implements mechanisms to work on notifications in alignment to business needs.
    /// </summary>
    /// <seealso cref="INotificationsManager" />
    /// <seealso cref="INotificationDelivery" />
    public class NotificationsManager : INotificationsManager, INotificationDelivery
    {
        /// <summary>
        /// The logger.
        /// </summary>
        private readonly ILogger<NotificationsManager> logger;

        /// <summary>
        /// The notifications repository.
        /// </summary>
        private readonly IRepository<WebNotificationItemEntity> notificationsRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationsManager"/> class.
        /// </summary>
        /// <param name="notificationsRepository">The intsnce for <see cref="IRepository{WebNotificationItemEntity}"/>.</param>
        /// <param name="logger">The instance for <see cref="ILogger{NotificationsManager}"/>.</param>
        /// <exception cref="ArgumentNullException">logger.</exception>
        /// <exception cref="ArgumentNullException">notificationsRepository.</exception>
        public NotificationsManager(IRepository<WebNotificationItemEntity> notificationsRepository, ILogger<NotificationsManager> logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.notificationsRepository = notificationsRepository ?? throw new ArgumentNullException(nameof(notificationsRepository));
        }

        /// <inheritdoc cref="INotificationsManager"/>
        public async Task MarkNotificationsAsReadAsync(string applicationName, IEnumerable<string> notificationIds)
        {
            if (string.IsNullOrWhiteSpace(applicationName))
            {
                throw new ArgumentException("The application name is not specified.", nameof(applicationName));
            }

            if (!(notificationIds?.Any() ?? false))
            {
                throw new ArgumentException("The notification identifiers are not specified.", nameof(notificationIds));
            }

            EntityCollection<WebNotificationItemEntity> webNotificationEntities = null;
            List<WebNotificationItemEntity> updatedReadStatusEntities;
            this.logger.LogInformation($"Started {nameof(this.MarkNotificationsAsReadAsync)} method of {nameof(NotificationsManager)}.");
            this.logger.LogInformation($"Total Notification Ids to process : {notificationIds.Count()}");
            webNotificationEntities = await this.LoadNotificationsWithIdsInternalAsync(notificationIds, applicationName, isTrackingIds: false).ConfigureAwait(false);
            if (webNotificationEntities != null && webNotificationEntities.Items.Any())
            {
                updatedReadStatusEntities = new List<WebNotificationItemEntity>();
                foreach (WebNotificationItemEntity webNotificationItemEntity in webNotificationEntities.Items)
                {
                    webNotificationItemEntity.ReadStatus = NotificationReadStatus.Read;
                    updatedReadStatusEntities.Add(webNotificationItemEntity);
                }

                _ = await this.notificationsRepository.UpsertAsync(updatedReadStatusEntities).ConfigureAwait(false);
            }

            this.logger.LogInformation($"Finished {nameof(this.MarkNotificationsAsReadAsync)} method of {nameof(NotificationsManager)}.");
        }

        /// <inheritdoc cref="INotificationsManager"/>
        public async Task<WebNotificationResponse> DeliverNotificationsAsync(string applicationName, string userObjectId)
        {
            if (string.IsNullOrWhiteSpace(applicationName))
            {
                throw new ArgumentException("The application name is not specified.", nameof(applicationName));
            }

            if (string.IsNullOrWhiteSpace(userObjectId))
            {
                throw new ArgumentException("The user object identifier is not specified.", nameof(userObjectId));
            }

            WebNotificationResponse webNotificationResponse = new WebNotificationResponse();
            this.logger.LogInformation($"Started {nameof(this.DeliverNotificationsAsync)} method of {nameof(NotificationsManager)}.");
            Expression<Func<WebNotificationItemEntity, bool>> filterExpression = NotificationsExpressionProvider.PrepareNotificationsFilter(applicationName, notificationReadStatus: null, userObjectId: userObjectId);
            EntityCollection<WebNotificationItemEntity> notificationCollection =
                await this.notificationsRepository.ReadAsync(
                filterExpression,
                NotificationsExpressionProvider.NotificationsSortOrderExpression,
                nextPageId: null,
                numOfEntities: BusinessConstants.PageSize).ConfigureAwait(false);
            webNotificationResponse.Notifications.AddRange(notificationCollection.Items.Select(wbn => wbn.ToWebNotification()));
            if (webNotificationResponse.Notifications.Any())
            {
                await this.MarkNotificationsDeliveredInternalAsync(NotificationDeliveryChannel.Web, notificationCollection.Items).ConfigureAwait(false);
            }

            this.logger.LogInformation($"Finished {nameof(this.DeliverNotificationsAsync)} method of {nameof(NotificationsManager)}.");
            return webNotificationResponse;
        }

        /// <inheritdoc cref="INotificationDelivery"/>
        public async Task MarkNotificationDeliveredAsync(IEnumerable<string> notificationIds, NotificationDeliveryChannel deliveryChannel, bool throwExceptions = true)
        {
            EntityCollection<WebNotificationItemEntity> notificationCollection;
            this.logger.LogInformation($"Started {nameof(this.MarkNotificationDeliveredAsync)} method of {nameof(NotificationsManager)}.");

            if (throwExceptions && notificationIds == null)
            {
                throw new ArgumentNullException(nameof(notificationIds));
            }
            else if (notificationIds != null)
            {
                if (!notificationIds.Any())
                {
                    this.logger.LogWarning("There are no notifications to mark delivered.");
                }
                else
                {
                    notificationCollection = await this.LoadNotificationsWithIdsInternalAsync(notificationIds).ConfigureAwait(false);
                    if (notificationCollection.Items != null)
                    {
                        await this.MarkNotificationsDeliveredInternalAsync(deliveryChannel, notificationCollection.Items).ConfigureAwait(false);
                    }
                    else
                    {
                        this.logger.LogWarning($"There are no notifications located with identifier(s) '{string.Join(',', notificationIds.ToArray())}'");
                    }
                }
            }

            this.logger.LogInformation($"Finished {nameof(this.MarkNotificationDeliveredAsync)} method of {nameof(NotificationsManager)}.");
        }

        /// <inheritdoc cref="INotificationsManager"/>
        public async Task<IEnumerable<WebNotification>> ProcessNotificationsAsync(string applicationName, IEnumerable<WebNotificationRequestItem> webNotificationRequestItems)
        {
            if (string.IsNullOrWhiteSpace(applicationName))
            {
                throw new ArgumentException("The application name is not specified.", nameof(applicationName));
            }

            if (webNotificationRequestItems is null || !webNotificationRequestItems.Any())
            {
                throw new ArgumentException("There is no web notification request item to process.");
            }

            this.logger.LogInformation($"Started {nameof(this.ProcessNotificationsAsync)} method of {nameof(NotificationsManager)}.");
            IEnumerable<WebNotification> notifications = await this.ProcessNotificationsInternalAsync(applicationName, webNotificationRequestItems).ConfigureAwait(false);
            this.logger.LogInformation($"Finished {nameof(this.ProcessNotificationsAsync)} method of {nameof(NotificationsManager)}.");
            return notifications;
        }

        /// <inheritdoc cref="INotificationsManager"/>
        public async Task<WebNotificationStatusResponse> LoadNotificationStatusAsync(string applicationName, IEnumerable<string> trackingIds)
        {
            this.logger.LogInformation($"Started {nameof(this.LoadNotificationStatusAsync)} method of {nameof(NotificationsManager)}.");
            if (string.IsNullOrWhiteSpace(applicationName))
            {
                throw new ArgumentException("The application name is not specified.", nameof(applicationName));
            }

            if (!(trackingIds?.Any() ?? false))
            {
                throw new ArgumentException("There are no tracking Ids.", nameof(trackingIds));
            }

            EntityCollection<WebNotificationItemEntity> webNotificationsCollection = await this.LoadNotificationsWithIdsInternalAsync(trackingIds, applicationName, isTrackingIds: true).ConfigureAwait(false);
            WebNotificationStatusResponse statusResponse = PrepareNotificationStatusResponse(trackingIds, webNotificationsCollection.Items);
            this.logger.LogInformation($"Finished {nameof(this.LoadNotificationStatusAsync)} method of {nameof(NotificationsManager)}.");
            return statusResponse;
        }

        /// <summary>
        /// Prepares the notification status response.
        /// </summary>
        /// <param name="trackingIds">The tracking ids.</param>
        /// <param name="webNotificationItemEntities">The web notification item entities.</param>
        /// <returns>The instance of <see cref="WebNotificationStatusResponse"/>.</returns>
        private static WebNotificationStatusResponse PrepareNotificationStatusResponse(IEnumerable<string> trackingIds, IEnumerable<WebNotificationItemEntity> webNotificationItemEntities)
        {
            Debug.Assert(trackingIds?.Any() ?? false, "Missing tracking Ids");
            WebNotificationStatus webNotificationStatus;
            WebNotificationStatusResponse statusResponse = new WebNotificationStatusResponse();
            IEnumerable<string> absentTrackingIds = LocateAbsentTrackingIds(trackingIds, webNotificationItemEntities);

            foreach (WebNotificationItemEntity webNotificationEntity in webNotificationItemEntities)
            {
                webNotificationStatus = new WebNotificationStatus
                {
                    TrackingId = webNotificationEntity.TrackingId,
                    IsValidTrackingId = true,
                    NotificationId = webNotificationEntity.NotificationId,
                    ReadStatus = webNotificationEntity.ReadStatus,
                };

                webNotificationStatus.DeliveryStatus[NotificationDeliveryChannel.Web.ToString().ToLowerInvariant()] = webNotificationEntity.DeliveredOnChannel[NotificationDeliveryChannel.Web];
                statusResponse.NotificationStatus.Add(webNotificationStatus);
            }

            foreach (string absentTrackingId in absentTrackingIds)
            {
                webNotificationStatus = new WebNotificationStatus
                {
                    IsValidTrackingId = false,
                    TrackingId = absentTrackingId,
                };

                statusResponse.NotificationStatus.Add(webNotificationStatus);
            }

            return statusResponse;
        }

        /// <summary>
        /// Locates the absent tracking ids.
        /// </summary>
        /// <param name="trackingIds">The tracking ids.</param>
        /// <param name="webNotificationItemEntities">The instance of <see cref="IEnumerable{WebNotificationItemEntity}"/>.</param>
        /// <returns>The instance of <see cref="IEnumerable{String}"/>.</returns>
        private static IEnumerable<string> LocateAbsentTrackingIds(IEnumerable<string> trackingIds, IEnumerable<WebNotificationItemEntity> webNotificationItemEntities)
        {
            Debug.Assert(trackingIds?.Any() ?? false, "Missing tracking Ids");
            Debug.Assert(webNotificationItemEntities?.Any() ?? false, "Missing web notification item entities.");
            List<string> absentTrackingIds = new List<string>();
            bool trackingIdAvailable = false;
            foreach (var trackingId in trackingIds)
            {
                foreach (var webNotificationEntity in webNotificationItemEntities)
                {
                    if (trackingId.Equals(webNotificationEntity.TrackingId, StringComparison.OrdinalIgnoreCase))
                    {
                        trackingIdAvailable = true;
                        break;
                    }
                }

                if (!trackingIdAvailable)
                {
                    absentTrackingIds.Add(trackingId);
                }

                trackingIdAvailable = false;
            }

            return absentTrackingIds;
        }

        /// <summary>
        /// Processes the notifications internally.
        /// </summary>
        /// <param name="applicationName">Name of the application.</param>
        /// <param name="webNotificationRequestItems">The instance for <see cref="IEnumerable{WebNotificationRequestItem}"/>.</param>
        /// <returns>The instance of <see cref="Task{WebNotification}"/> representing an asynchronous operation.</returns>
        private async Task<IEnumerable<WebNotification>> ProcessNotificationsInternalAsync(string applicationName, IEnumerable<WebNotificationRequestItem> webNotificationRequestItems)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(applicationName), "Missing application name.");
            Debug.Assert(webNotificationRequestItems?.Any() ?? false, "Missing web notification request items.");
            IEnumerable<WebNotificationItemEntity> notificationEntities = webNotificationRequestItems.Select(wbr => wbr.ToEntity(applicationName)).ToList();
            await this.UpdateNotificationEntitiesWithExitingIdsAsync(notificationEntities).ConfigureAwait(false);
            notificationEntities = await this.notificationsRepository.UpsertAsync(notificationEntities).ConfigureAwait(false);
            IOrderedQueryable<WebNotificationItemEntity> queryableNotifications = notificationEntities.AsQueryable().Where(NotificationsExpressionProvider.PrepareNotificationsFilter(applicationName, NotificationReadStatus.New)).OrderBy(NotificationsExpressionProvider.NotificationsSortOrderExpression);
            IEnumerable<WebNotification> notifications = queryableNotifications.Select(we => we.ToWebNotification()).ToList();
            return notifications;
        }

        /// <summary>
        /// Updates the notification entities with exiting ids asynchronously if any.
        /// </summary>
        /// <param name="webNotificationItemEntities">The instance for <see cref="IEnumerable{WebNotificationItemEntity}"/>.</param>
        /// <returns>The instance of <see cref="Task"/> representing an asynchronous operation.</returns>
        private async Task UpdateNotificationEntitiesWithExitingIdsAsync(IEnumerable<WebNotificationItemEntity> webNotificationItemEntities)
        {
            Debug.Assert(webNotificationItemEntities?.Any() ?? false, "Missing web notification entities.");
            IEnumerable<string> notificationIds = webNotificationItemEntities.Select(wbn => wbn.NotificationId);
            EntityCollection<WebNotificationItemEntity> notificationCollection = await this.LoadNotificationsWithIdsInternalAsync(notificationIds).ConfigureAwait(false);
            foreach (var wbn in webNotificationItemEntities)
            {
                WebNotificationItemEntity notificationItemEntity = notificationCollection.Items
                        .Where(notif => notif.NotificationId.Equals(wbn.NotificationId, StringComparison.Ordinal))
                        .FirstOrDefault();
                if (notificationItemEntity != null && !string.IsNullOrWhiteSpace(notificationItemEntity.Id))
                {
                    wbn.Id = notificationItemEntity.Id;
                }
            }
        }

        /// <summary>
        /// Loads the notifications with ids asynchronously.
        /// </summary>
        /// <param name="ids">The notification related ids.</param>
        /// <param name="applicationName">Name of the application.</param>
        /// <param name="isTrackingIds"><c>true</c> if the specified Ids are tracking identifiers.</param>
        /// <returns>The instance of <see cref="Task{TResult}"/>.</returns>
        private async Task<EntityCollection<WebNotificationItemEntity>> LoadNotificationsWithIdsInternalAsync(IEnumerable<string> ids, string applicationName = null, bool isTrackingIds = false)
        {
            Debug.Assert(ids?.Any() ?? false, "Missing ids");
            Expression<Func<WebNotificationItemEntity, bool>> filterExpression = NotificationsExpressionProvider.PrepareNotificationsFilter(ids, applicationName, isTrackingIds);
            EntityCollection<WebNotificationItemEntity> notificationCollection =
                await this.notificationsRepository.ReadAsync(
                    filterExpression,
                    NotificationsExpressionProvider.NotificationsSortOrderExpression,
                    nextPageId: null,
                    numOfEntities: ids.Count())
                .ConfigureAwait(false);
            return notificationCollection;
        }

        /// <summary>
        /// Marks the notifications delivered asynchronously.
        /// </summary>
        /// <param name="deliveryChannel">The value from <see cref="NotificationDeliveryChannel"/> enum.</param>
        /// <param name="notifications">The notifications.</param>
        /// <returns>The instance of <see cref="Task"/> representing an asynchronous operation.</returns>
        private async Task MarkNotificationsDeliveredInternalAsync(NotificationDeliveryChannel deliveryChannel, IEnumerable<WebNotificationItemEntity> notifications)
        {
            Debug.Assert(notifications?.Any() ?? false, "Missing notifications.");
            List<WebNotificationItemEntity> webNotificationItemEntities = new List<WebNotificationItemEntity>();
            foreach (var notificationItemEntity in notifications)
            {
                if (!notificationItemEntity.DeliveredOnChannel[deliveryChannel])
                {
                    notificationItemEntity.DeliveredOnChannel[deliveryChannel] = true;
                    webNotificationItemEntities.Add(notificationItemEntity);
                }
            }

            _ = await this.notificationsRepository.UpsertAsync(webNotificationItemEntities).ConfigureAwait(false);
        }
    }
}
