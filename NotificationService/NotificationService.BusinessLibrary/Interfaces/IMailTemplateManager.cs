// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.BusinessLibrary
{
    using System.Threading.Tasks;
    using NotificationService.Contracts;

    /// <summary>
    /// Interface for mail template manager.
    /// </summary>
    public interface IMailTemplateManager
    {
        /// <summary>
        /// Saves email template.
        /// </summary>
        /// <param name="applicationName">Application sourcing the email template.</param>
        /// <param name="mailTempalte">Mail template item.</param>
        /// <returns>Success or Failure.</returns>
        Task<bool> SaveEmailTemplate(string applicationName, MailTemplate mailTempalte);

        /// <summary>
        /// Gets the email template entities from database.
        /// </summary>
        /// <param name="applicationName">Application sourcing the email template.</param>
        /// <param name="templateName">Mail template name.</param>
        /// <returns><see cref="MailTemplate"/>.</returns>
        Task<MailTemplate> GetMailTemplate(string applicationName, string templateName);

        /// <summary>
        /// Deletes email template.
        /// </summary>
        /// <param name="applicationName">Application sourcing the email template.</param>
        /// <param name="templateName">Mail template name.</param>
        /// <returns>Success or Failure.</returns>
        Task<bool> DeleteMailTemplate(string applicationName, string templateName);
    }
}
