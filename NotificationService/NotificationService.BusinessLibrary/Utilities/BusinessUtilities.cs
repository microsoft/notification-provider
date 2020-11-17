// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.BusinessLibrary
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Newtonsoft.Json;
    using NotificationService.Contracts;

    /// <summary>
    /// Static class with utility methods.
    /// </summary>
    public static class BusinessUtilities
    {
        /// <summary>
        /// Breaks the input list to multiple chunks each of size provided as input.
        /// </summary>
        /// <typeparam name="T">Type of object in the List.</typeparam>
        /// <param name="listItems">List of objects.</param>
        /// <param name="nSize">Chunk size.</param>
        /// <returns>An enumerable collection of chunks.</returns>
        public static IEnumerable<List<T>> SplitList<T>(List<T> listItems, int nSize = 4)
        {
            if (listItems is null)
            {
                throw new ArgumentNullException(nameof(listItems));
            }

            for (int i = 0; i < listItems.Count; i += nSize)
            {
                yield return listItems.GetRange(i, Math.Min(nSize, listItems.Count - i));
            }
        }

        public static IList<string> GetCloudMessagesForEntities(string applicationName, IList<EmailNotificationItemEntity> notificationItemEntities, bool ignoreAlreadySent = true)
        {
            IList<string> cloudMessages = new List<string>();
            var cloudMessage = new
            {
                NotificationIds = notificationItemEntities.Select(nie => nie.NotificationId).ToArray(),
                Application = applicationName,
                NotificationType = NotificationType.Mail,
                IgnoreAlreadySent = ignoreAlreadySent,
            };
            cloudMessages.Add(JsonConvert.SerializeObject(cloudMessage));
            return cloudMessages;
        }

        /// <summary>
        /// Converts the input notification ids to Cloud messages to be queued.
        /// </summary>
        /// <param name="applicationName">Name of associated application.</param>
        /// <param name="notificationIds">Array of notification Ids.</param>
        /// <param name="ignoreAlreadySent">Boolean value to ignore already sent messages.</param>
        /// <returns>List of cloud messages.</returns>
        public static IList<string> GetCloudMessagesForIds(string applicationName, string[] notificationIds, bool ignoreAlreadySent = false)
        {
            IList<string> cloudMessages = new List<string>();
            var cloudMessage = new
            {
                NotificationIds = notificationIds,
                Application = applicationName,
                NotificationType = NotificationType.Mail,
                IgnoreAlreadySent = ignoreAlreadySent,
            };
            cloudMessages.Add(JsonConvert.SerializeObject(cloudMessage));
            return cloudMessages;
        }
    }
}
