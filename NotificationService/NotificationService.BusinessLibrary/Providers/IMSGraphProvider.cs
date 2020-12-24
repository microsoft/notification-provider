// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.BusinessLibrary
{
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using NotificationService.Contracts;
    using NotificationService.Contracts.Models.Graph.Invite;

    /// <summary>
    /// MS Graph Provider interface.
    /// </summary>
    public interface IMSGraphProvider
    {
        /// <summary>
        /// Sends the email message to recipients via Graph API.
        /// </summary>
        /// <param name="authenticationHeaderValue">Authentication header corresponding to the sender of the email.</param>
        /// <param name="emailMessage">Email message to be sent.</param>
        /// <param name="notificationId">Internal identifier of the email message to be sent.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task<bool> SendEmailNotification(AuthenticationHeaderValue authenticationHeaderValue, EmailMessagePayload emailMessage, string notificationId);

        /// <summary>
        /// Sends a batch of graph request to Graph API.
        /// </summary>
        /// <param name="authenticationHeaderValue">Authentication header corresponding to the sender of the email.</param>
        /// <param name="graphBatchRequest">Batch request to be processed.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task<IList<NotificationBatchItemResponse>> ProcessEmailRequestBatch(AuthenticationHeaderValue authenticationHeaderValue, GraphBatchRequest graphBatchRequest);

        /// <summary>
        /// Send Meeting invite to recipients via Graph API.
        /// </summary>
        /// <param name="authenticationHeaderValue">Authentication header corresponding to the sender of the email.</param>
        /// <param name="payLoad">Meeting invite Paylod to be sent.</param>
        /// <param name="notificationId"> Internal identifier of the email message to be sent.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task<bool> SendMeetingInvite(AuthenticationHeaderValue authenticationHeaderValue, InvitePayload payLoad, string notificationId);
    }
}
