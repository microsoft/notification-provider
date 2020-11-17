// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.BusinessLibrary.Providers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using NotificationService.Common.Utility;
    using NotificationService.Contracts;
    using NotificationService.Contracts.Entities.Web;

    /// <summary>
    /// The <see cref="NotificationsExpressionProvider"/> class contains reusable expressions or enables to prepare expressions.
    /// </summary>
    internal static class NotificationsExpressionProvider
    {
        /// <summary>
        /// Gets the notifications sort order expression.
        /// </summary>
        /// <value>
        /// The notifications sort order expression.
        /// </value>
        public static Expression<Func<WebNotificationItemEntity, NotificationPriority>> NotificationsSortOrderExpression => notification => notification.Priority;

        /// <summary>
        /// Prepares the notifications filter.
        /// </summary>
        /// <param name="applicationName">Name of the application.</param>
        /// <param name="notificationReadStatus">The notification read status.</param>
        /// <param name="userObjectId">The user object identifier.</param>
        /// <returns>The filter expression.</returns>
        public static Expression<Func<WebNotificationItemEntity, bool>> PrepareNotificationsFilter(string applicationName, NotificationReadStatus? notificationReadStatus, string userObjectId = null)
        {
            Expression<Func<WebNotificationItemEntity, bool>> filterExpression = notification => notification.Application == applicationName
                && notification.PublishOnUTCDate < DateTime.UtcNow
                && notification.ExpiresOnUTCDate > DateTime.UtcNow;
            if (notificationReadStatus != null)
            {
                filterExpression = filterExpression.And(notification => notification.ReadStatus == notificationReadStatus);
            }

            if (!string.IsNullOrWhiteSpace(userObjectId))
            {
                filterExpression = filterExpression.And(notification => notification.Recipient.ObjectIdentifier == userObjectId);
            }

            return filterExpression;
        }

        /// <summary>
        /// Prepares the notifications filter based on notification identifiers or tracking Ids..
        /// </summary>
        /// <param name="ids">The notification related identifiers.</param>
        /// <param name="applicationName">Name of the application.</param>
        /// <param name="isTrackingIds"><c>true</c> to generate tracking Id based filter.</param>
        /// <returns>The filter expression.</returns>
        public static Expression<Func<WebNotificationItemEntity, bool>> PrepareNotificationsFilter(IEnumerable<string> ids, string applicationName = null, bool isTrackingIds = false)
        {
            Expression<Func<WebNotificationItemEntity, bool>> filterExpression = null;
            if (!(ids?.Any() ?? false))
            {
                throw new ArgumentNullException(nameof(ids));
            }

            if (!string.IsNullOrWhiteSpace(applicationName))
            {
                filterExpression = notification => notification.Application == applicationName;
            }

            if (isTrackingIds)
            {
                if (filterExpression == null)
                {
                    filterExpression = notification => ids.Contains(notification.TrackingId);
                }
                else
                {
                    filterExpression = filterExpression.And(notification => ids.Contains(notification.TrackingId));
                }
            }
            else
            {
                if (filterExpression == null)
                {
                    filterExpression = notification => ids.Contains(notification.NotificationId);
                }
                else
                {
                    filterExpression = filterExpression.And(notification => ids.Contains(notification.NotificationId));
                }
            }

            return filterExpression;
        }
    }
}
