// <copyright file="IEmailService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace DirectSend
{
    using System.Threading.Tasks;
    using DirectSend.Models.Mail;

    /// <summary>
    /// IEmailService.
    /// </summary>
    public interface IEmailService
    {
        /// <summary>
        /// Sends the asynchronous.
        /// </summary>
        /// <param name="email">The EmailMessage.</param>
        /// <returns>A <see cref="Task"/>.</returns>
        Task SendEmailAsync(EmailMessage email);

        /// <summary>
        /// Sends the asynchronous.
        /// </summary>
        /// <param name="email">The EmailMessage.</param>
        /// <returns>A <see cref="Task"/>.</returns>
        Task SendMeetingInviteAsync(EmailMessage email);
    }
}
