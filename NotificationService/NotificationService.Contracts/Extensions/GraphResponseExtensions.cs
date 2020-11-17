// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Contracts
{
    /// <summary>
    /// Extensions of <see cref="GraphResponse"/>.
    /// </summary>
    public static class GraphResponseExtensions
    {
        /// <summary>
        /// Converts <see cref="GraphResponse"/> to <see cref="NotificationBatchItemResponse"/>.
        /// </summary>
        /// <param name="graphResponse">Graph Response.</param>
        /// <returns><see cref="NotificationBatchItemResponse"/>.</returns>
        public static NotificationBatchItemResponse ToNotificationBatchItemResponse(this GraphResponse graphResponse) => new NotificationBatchItemResponse()
        {
            NotificationId = graphResponse?.Id,
            Status = graphResponse.Status,
            Error = graphResponse?.Body?.Error?.Messsage,
        };
    }
}
