// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.BusinessLibrary
{
    using System.Collections.Generic;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using NotificationService.BusinessLibrary.Models;
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
        Task<ResponseData<string>> SendEmailNotification(AuthenticationHeaderValue authenticationHeaderValue, EmailMessagePayload emailMessage, string notificationId);

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
        Task<ResponseData<string>> SendMeetingInvite(AuthenticationHeaderValue authenticationHeaderValue, InvitePayload payLoad, string notificationId);

        /// <summary>
        /// Sends attachments for already sent Event/Invite.
        /// </summary>
        /// <param name="authenticationHeaderValue">Authentication header corresponding to the sender of the email.</param>
        /// <param name="attachments">List of Attachment object to be sent. </param>
        /// <param name="eventId">EventId as reference to already sent event.</param>
        /// <param name="notificationId"> Internal identifier of the email message to be sent. </param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        IDictionary<string, ResponseData<string>> SendMeetingInviteAttachments(AuthenticationHeaderValue authenticationHeaderValue, List<FileAttachment> attachments, string eventId, string notificationId);

        /// <summary>
        ///  Deletes an invite.
        /// </summary>
        /// <param name="authenticationHeaderValue"> Authentication header corresponding to the sender of the email. </param>
        /// <param name="notificationId"> Internal identifier of the email message to be sent. </param>
        /// <param name="eventId"> EventId as reference to already sent event. </param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task<ResponseData<string>> DeleteMeetingInvite(AuthenticationHeaderValue authenticationHeaderValue, string notificationId, string eventId);

        /// <summary>
        /// Updates Invite.
        /// </summary>
        /// <param name="authenticationHeaderValue"> Authentication header corresponding to the sender of the email. </param>
        /// <param name="payLoad"> Meeting invite Paylod to be sent. </param>
        /// <param name="notificationId"> Internal identifier of the email message to be sent.</param>
        /// <param name="eventId">EventId as reference to already sent event.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task<ResponseData<string>> UpdateMeetingInvite(AuthenticationHeaderValue authenticationHeaderValue, InvitePayload payLoad, string notificationId, string eventId);
    }
}
