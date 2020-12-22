// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.UnitTests.Mocks
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using NotificationService.BusinessLibrary.Interfaces;
    using NotificationService.Contracts;
    using NotificationService.Contracts.Entities;

    /// <summary>
    /// Mock Notification Provider Class.
    /// </summary>
    /// <seealso cref="NotificationService.BusinessLibrary.Interfaces.INotificationProvider" />
    public class MockNotificationProvider : INotificationProvider
    {
        /// <inheritdoc/>
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task ProcessMeetingNotificationEntities(string applicationName, IList<MeetingNotificationItemEntity> notificationEntities)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            foreach (var item in notificationEntities)
            {
                if (item.RequiredAttendees == "user@contoso.com")
                {
                    item.Status = NotificationItemStatus.Sent;
                }
            }
        }

        /// <inheritdoc/>
        public Task ProcessNotificationEntities(string applicationName, IList<EmailNotificationItemEntity> notificationEntities) => throw new NotImplementedException();
    }
}
