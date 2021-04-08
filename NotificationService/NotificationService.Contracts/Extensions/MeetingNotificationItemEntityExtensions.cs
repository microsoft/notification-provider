// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Contracts.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using NotificationService.Contracts.Entities;
    using NotificationService.Contracts.Models;

    /// <summary>
    /// MeetingNotificationItemEntityExtensions.
    /// </summary>
    public static class MeetingNotificationItemEntityExtensions
    {
        /// <summary>
        /// Converts <see cref="MeetingNotificationItemEntity"/> to a <see cref="MeetingInviteReportResponse"/>.
        /// </summary>
        /// <param name="meetingNotificationItemEntity"> meetingNotificationItemEntity. </param>
        /// <returns><see cref="MeetingInviteReportResponse"/>.</returns>
        public static MeetingInviteReportResponse ToMeetingInviteReportResponse(MeetingNotificationItemEntity meetingNotificationItemEntity)
        {
            return new MeetingInviteReportResponse()
            {
                NotificationId = meetingNotificationItemEntity?.NotificationId,
                Application = meetingNotificationItemEntity?.Application,
                EmailAccountUsed = meetingNotificationItemEntity?.EmailAccountUsed,
                TrackingId = meetingNotificationItemEntity?.TrackingId,
                Status = meetingNotificationItemEntity?.Status.ToString(),
                Priority = meetingNotificationItemEntity?.Priority.ToString(),
                TryCount = meetingNotificationItemEntity?.TryCount ?? 0,
                ErrorMessage = meetingNotificationItemEntity?.ErrorMessage,
                CreatedDateTime = meetingNotificationItemEntity?.CreatedDateTime ?? DateTime.MinValue,
                UpdatedDateTime = meetingNotificationItemEntity?.UpdatedDateTime ?? DateTime.MinValue,
                SendOnUtcDate = meetingNotificationItemEntity?.SendOnUtcDate ?? DateTime.MinValue,
                RequiredAttendees = meetingNotificationItemEntity?.RequiredAttendees,
                From = meetingNotificationItemEntity?.From,
                OptionalAttendees = meetingNotificationItemEntity?.OptionalAttendees,
                Subject = meetingNotificationItemEntity?.Subject,
            };
        }
    }
}
