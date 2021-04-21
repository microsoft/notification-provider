// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.BusinessLibrary
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Threading.Tasks;
    using NotificationService.Common.Encryption;
    using NotificationService.Common.Logger;
    using NotificationService.Contracts;
    using NotificationService.Contracts.Extensions;
    using NotificationService.Data;

    /// <summary>
    /// Business Manager for processing mail template.
    /// </summary>
    /// <seealso cref="IMailTemplateManager" />
    public class MailTemplateManager : IMailTemplateManager
    {
        private readonly ILogger logger;
        private readonly IMailTemplateRepository mailTemplateRepository;
        private readonly IEncryptionService encryptionService;

        /// <summary>
        /// Initializes a new instance of the <see cref="MailTemplateManager"/> class.
        /// </summary>
        /// <param name="logger"><see cref="ILogger"/> instance.</param>
        /// <param name="mailTemplateRepository"><see cref="IMailTemplateRepository"/> instance.</param>
        /// <param name="encryptionService"><see cref="IEncryptionService"/> instance.</param>
        public MailTemplateManager(
            ILogger logger,
            IMailTemplateRepository mailTemplateRepository,
            IEncryptionService encryptionService)
        {
            this.logger = logger;
            this.mailTemplateRepository = mailTemplateRepository;
            this.encryptionService = encryptionService;
        }

        /// <inheritdoc/>
        public async Task<MailTemplate> GetMailTemplate(string applicationName, string templateName)
        {
            if (string.IsNullOrWhiteSpace(applicationName))
            {
                throw new ArgumentException("Application Name cannot be null or empty.", nameof(applicationName));
            }

            if (string.IsNullOrWhiteSpace(templateName))
            {
                throw new ArgumentException("Template Name cannot be null or empty.", nameof(templateName));
            }

            var traceprops = new Dictionary<string, string>();
            traceprops[AIConstants.Application] = applicationName;
            traceprops[AIConstants.MailTemplateName] = templateName;
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            bool result = false;
            this.logger.WriteCustomEvent($"{nameof(this.GetMailTemplate)} Started", traceprops);

            try
            {
                this.logger.TraceInformation($"Started {nameof(this.GetMailTemplate)} method of {nameof(MailTemplateManager)}.");
                var mailTemplateEntity = await this.mailTemplateRepository.GetMailTemplate(applicationName, templateName).ConfigureAwait(false);

                var response = mailTemplateEntity.ToContract(this.encryptionService);
                this.logger.TraceInformation($"Finished {nameof(this.GetMailTemplate)} method of {nameof(MailTemplateManager)}.");
                result = true;
                return response;
            }
            catch (Exception ex)
            {
                result = false;
                this.logger.WriteException(ex);
                throw; // controller methods logs this exception
            }
            finally
            {
                stopwatch.Stop();
                traceprops[AIConstants.Result] = result.ToString(CultureInfo.InvariantCulture);
                var metrics = new Dictionary<string, double>();
                metrics[AIConstants.Duration] = stopwatch.ElapsedMilliseconds;
                this.logger.WriteCustomEvent($"{nameof(this.GetMailTemplate)} Completed", traceprops, metrics);
            }
        }

        /// <inheritdoc/>
        public async Task<bool> SaveEmailTemplate(string applicationName, MailTemplate mailTempalte)
        {
            if (string.IsNullOrWhiteSpace(applicationName))
            {
                throw new ArgumentException("Application Name cannot be null or empty.", nameof(applicationName));
            }

            if (mailTempalte is null)
            {
                throw new ArgumentException("Mail template param should not be null.", nameof(mailTempalte));
            }

            bool response = false;
            bool result = false;
            var traceprops = new Dictionary<string, string>();
            traceprops[AIConstants.Application] = applicationName;
            traceprops[AIConstants.MailTemplateId] = mailTempalte.TemplateId;
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            this.logger.WriteCustomEvent($"{nameof(this.SaveEmailTemplate)} Started", traceprops);

            try
            {
                this.logger.TraceInformation($"Started {nameof(this.SaveEmailTemplate)} method of {nameof(MailTemplateManager)}.", traceprops);

                var mailTempalteEntity = mailTempalte.ToEntity(applicationName, this.encryptionService);
                response = await this.mailTemplateRepository.UpsertEmailTemplateEntities(mailTempalteEntity).ConfigureAwait(false);
                this.logger.TraceInformation($"Finished {nameof(this.SaveEmailTemplate)} method of {nameof(MailTemplateManager)}.", traceprops);
                result = true;
                return true;
            }
            catch (Exception ex)
            {
                result = false;
                this.logger.WriteException(ex);
                throw; // controller methods logs this exception
            }
            finally
            {
                stopwatch.Stop();
                traceprops[AIConstants.Result] = result.ToString(CultureInfo.InvariantCulture);
                var metrics = new Dictionary<string, double>();
                metrics[AIConstants.Duration] = stopwatch.ElapsedMilliseconds;
                this.logger.WriteCustomEvent($"{nameof(this.SaveEmailTemplate)} Completed", traceprops, metrics);
            }
        }

        /// <summary>
        /// Deletes the mail template.
        /// </summary>
        /// <param name="applicationName"> application sourcing the template.</param>
        /// <param name="templateName"> template name.</param>
        /// <returns>"status of delete operation"/>.</returns>
        public async Task<bool> DeleteMailTemplate(string applicationName, string templateName)
        {
            if (string.IsNullOrWhiteSpace(applicationName))
            {
                throw new ArgumentException("Application Name cannot be null or empty.", nameof(applicationName));
            }

            if (string.IsNullOrWhiteSpace(templateName))
            {
                throw new ArgumentException("Template Name cannot be null or empty.", nameof(templateName));
            }

            var traceprops = new Dictionary<string, string>();
            traceprops[AIConstants.Application] = applicationName;
            traceprops[AIConstants.MailTemplateName] = templateName;
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            bool result = false;
            this.logger.WriteCustomEvent($"{nameof(this.DeleteMailTemplate)} Started", traceprops);

            try
            {
                this.logger.TraceInformation($"Started {nameof(this.DeleteMailTemplate)} method of {nameof(MailTemplateManager)}.", traceprops);
                result = await this.mailTemplateRepository.DeleteMailTemplate(applicationName, templateName).ConfigureAwait(false);
                this.logger.TraceInformation($"Finished {nameof(this.DeleteMailTemplate)} method of {nameof(MailTemplateManager)}.", traceprops);
                return result;
            }
            catch (Exception ex)
            {
                this.logger.WriteException(ex);
                result = false;
                throw; // controller methods logs this exception
            }
            finally
            {
                stopwatch.Stop();
                CultureInfo provider = new CultureInfo("en-us");
                traceprops[AIConstants.Result] = result.ToString(provider);
                var metrics = new Dictionary<string, double>();
                metrics[AIConstants.Duration] = stopwatch.ElapsedMilliseconds;
                this.logger.WriteCustomEvent($"{nameof(this.DeleteMailTemplate)} Completed", traceprops, metrics);
            }
        }
    }
}
