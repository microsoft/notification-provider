// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Data
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using NotificationService.Contracts.Entities;

    /// <summary>
    /// Repository Interface for Email Template Items.
    /// </summary>
    public interface IMailTemplateRepository
    {
        /// <summary>
        /// Saves the changes on mail template entities into database.
        /// </summary>
        /// <param name="mailTemplateEntity"><see cref="MailTemplateEntity"/>.</param>
        /// <returns>Sucess or failure.</returns>
        Task<bool> UpsertEmailTemplateEntities(MailTemplateEntity mailTemplateEntity);

        /// <summary>
        /// Gets the mail template entities from database.
        /// </summary>
        /// <param name="applicationName">Application sourcing the email template.</param>
        /// <param name="templateName">Mail template name.</param>
        /// <returns><see cref="MailTemplateEntity"/>.</returns>
        Task<MailTemplateEntity> GetMailTemplate(string applicationName, string templateName);

        /// <summary>
        /// All template entities from the table for an application would be returned.
        /// </summary>
        /// <param name="applicationName">Application sourcing the email template.</param>
        /// <returns><see cref="MailTemplateEntity"/> list of mail template entities .</returns>
        Task<IList<MailTemplateEntity>> GetAllTemplateEntities(string applicationName);

        /// <summary>
        /// Deletes Email template.
        /// </summary>
        /// <param name="applicationName">Application sourcing the email template.</param>
        /// <param name="templateName">Mail template name.</param>
        /// <returns><see cref="Task"/> status of delete template. </returns>
        Task<bool> DeleteMailTemplate(string applicationName, string templateName);
    }
}
