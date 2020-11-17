// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

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
        Task SendAsync(EmailMessage email);
    }
}
