﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.BusinessLibrary
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;
    using NotificationService.Common;
    using NotificationService.Contracts;
    using NotificationService.Contracts.Entities;

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

        /// <summary>
        /// Gets the cloud messages for entities.
        /// </summary>
        /// <param name="applicationName">Name of the application.</param>
        /// <param name="notificationItemEntities">The notification item entities.</param>
        /// <param name="ignoreAlreadySent">if set to <c>true</c> [ignore already sent].</param>
        /// <returns>A List of <see cref="string"/>.</returns>
        public static IList<string> GetCloudMessagesForEntities(string applicationName, IList<EmailNotificationItemEntity> notificationItemEntities, bool ignoreAlreadySent = true)
        {
            IList<string> cloudMessages = new List<string>();

            List<List<EmailNotificationItemEntity>> batchesToQueue = SplitList<EmailNotificationItemEntity>(notificationItemEntities.ToList(), ApplicationConstants.BatchSizeToStore).ToList();

            foreach (var batch in batchesToQueue)
            {
                var cloudMessage = new
                {
                    NotificationIds = batch.Select(nie => nie.NotificationId).ToArray(),
                    Application = applicationName,
                    NotificationType = NotificationType.Mail,
                    IgnoreAlreadySent = ignoreAlreadySent,
                };
                cloudMessages.Add(JsonConvert.SerializeObject(cloudMessage));
            }

            return cloudMessages;
        }

        /// <summary>
        /// Gets the cloud messages for entities.
        /// </summary>
        /// <param name="applicationName">Name of the application.</param>
        /// <param name="notificationItemEntities">The notification item entities.</param>
        /// <param name="ignoreAlreadySent">if set to <c>true</c> [ignore already sent].</param>
        /// <returns>A List of <see cref="string"/>.</returns>
        public static IList<string> GetCloudMessagesForEntities(string applicationName, IList<MeetingNotificationItemEntity> notificationItemEntities, bool ignoreAlreadySent = true)
        {
            IList<string> cloudMessages = new List<string>();
            List<List<MeetingNotificationItemEntity>> batchesToQueue = SplitList<MeetingNotificationItemEntity>(notificationItemEntities.ToList(), ApplicationConstants.BatchSizeToStore).ToList();

            foreach (var batch in batchesToQueue)
            {
                var cloudMessage = new
                {
                    NotificationIds = batch.Select(nie => nie.NotificationId).ToArray(),
                    Application = applicationName,
                    NotificationType = NotificationType.Meet,
                    IgnoreAlreadySent = ignoreAlreadySent,
                };
                cloudMessages.Add(JsonConvert.SerializeObject(cloudMessage));
            }

            return cloudMessages;
        }

        /// <summary>
        /// Converts the input notification ids to Cloud messages to be queued.
        /// </summary>
        /// <param name="applicationName">Name of associated application.</param>
        /// <param name="notificationIds">Array of notification Ids.</param>
        /// <param name="notifType">Type of notification (Mail, Meeting).</param>
        /// <param name="ignoreAlreadySent">Boolean value to ignore already sent messages.</param>
        /// <returns>List of cloud messages.</returns>
        public static IList<string> GetCloudMessagesForIds(string applicationName, string[] notificationIds, NotificationType notifType, bool ignoreAlreadySent = false)
        {
            IList<string> cloudMessages = new List<string>();

            var batcharrays = SplitArray<string>(notificationIds, ApplicationConstants.BatchSizeToStore);

            foreach (var batch in batcharrays)
            {
                var cloudMessage = new
                {
                    NotificationIds = batch,
                    Application = applicationName,
                    NotificationType = notifType,
                    IgnoreAlreadySent = ignoreAlreadySent,
                };
                cloudMessages.Add(JsonConvert.SerializeObject(cloudMessage));
            }

            return cloudMessages;
        }

        /// <summary>
        /// Splits an array into several smaller arrays.
        /// </summary>
        /// <typeparam name="T">The type of the array.</typeparam>
        /// <param name="array">The array to split.</param>
        /// <param name="size">The size of the smaller arrays.</param>
        /// <returns>An array containing smaller arrays.</returns>
        public static IEnumerable<IEnumerable<T>> SplitArray<T>(this T[] array, int size = ApplicationConstants.BatchSizeToStore)
        {
            if (array is null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            for (var i = 0; i < (float)array.Length / size; i++)
            {
                yield return array.Skip(i * size).Take(size);
            }
        }
    }
}
