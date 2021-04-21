// <copyright file="DirectSendMailService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace DirectSend
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Net.Sockets;
    using System.Threading.Tasks;
    using DirectSend.Models.Configurations;
    using DirectSend.Models.Mail;
    using MailKit;
    using MailKit.Net.Smtp;
    using MimeKit;
    using MimeKit.Text;
    using NotificationProviders.Common.Logger;

    // for info on SmtpClient thread safety, see https://www.infoq.com/news/2017/04/MailKit-MimeKit-Official/

    /// <summary>
    /// DirectSendMailService.
    /// </summary>
    /// <seealso cref="IEmailService" />
    public class DirectSendMailService : IEmailService
    {
        private readonly ISmtpClientPool clientPool;
        private readonly ILogger logger;
        private readonly SendAccountConfiguration mailConfiguration;

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectSendMailService"/> class.
        /// </summary>
        /// <param name="clientPool">The client pool.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="mailConfiguration">The mail configuration.</param>
        public DirectSendMailService(
            ISmtpClientPool clientPool,
            ILogger logger,
            SendAccountConfiguration mailConfiguration)
        {
            this.clientPool = clientPool;
            this.logger = logger;
            this.mailConfiguration = mailConfiguration;
        }

        /// <summary>
        /// Sends the asynchronous.
        /// </summary>
        /// <param name="emailMessage">The EmailMessage.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task SendEmailAsync(EmailMessage emailMessage)
        {
            if (emailMessage == null)
            {
                throw new ArgumentNullException(nameof(emailMessage));
            }

            string recipients = string.Join(",", emailMessage.ToAddresses.Select(r => r.Address).ToList());

            var traceProps = new Dictionary<string, string>();
            traceProps["OperationName"] = this.GetType().FullName;
            traceProps["Recipients"] = recipients;

            var trimmedAddresses = emailMessage.ToAddresses.Where(a => !string.IsNullOrEmpty(a.Address)).ToArray();
            emailMessage.ToAddresses = trimmedAddresses;

            if (!emailMessage.ToAddresses.Any())
            {
                ArgumentNullException ex = new ArgumentNullException("ToAddresses", "Email message is missing 'To' addresses.");
                this.logger.WriteException(ex, traceProps);
                return;
            }

            var message = new MimeMessage();
            message.To.AddRange(emailMessage.ToAddresses.Select(x => new MailboxAddress(x.Name, x.Address)));
            if (emailMessage.CcAddresses != null && emailMessage.CcAddresses.Any())
            {
                message.Cc.AddRange(emailMessage.CcAddresses.Select(x => new MailboxAddress(x.Name, x.Address)));
            }

            if (emailMessage.ReplyTo != null && emailMessage.ReplyTo.Any())
            {
                message.ReplyTo.AddRange(emailMessage.ReplyTo.Select(x => new MailboxAddress(x.Name, x.Address)));
            }

            message.From.AddRange(emailMessage.FromAddresses.Select(x => new MailboxAddress(this.mailConfiguration.FromAddressDisplayName, x.Address)));
            message.Importance = (MimeKit.MessageImportance)emailMessage.Importance;

            message.Subject = emailMessage.Subject;

            // We will say we are sending HTML. But there are options for plaintext etc.
            if (emailMessage.FileContent != null && emailMessage.FileName != null && emailMessage.FileContent.Any() && emailMessage.FileContent.Count() == emailMessage.FileName.Count())
            {
                TextPart body = new TextPart(TextFormat.Html)
                {
                    Text = emailMessage.Content,
                };

                Multipart multipartContent = new Multipart("mixed");
                multipartContent.Add(body);

                for (var i = 0; i <= emailMessage.FileName.Count() - 1; i++)
                {
                    byte[] fileContent = Convert.FromBase64String(emailMessage.FileContent.ElementAt(i));

                    MemoryStream stream = new MemoryStream(fileContent);

                    MimePart attachment = new MimePart("text", "octet-stream")
                    {
                        Content = new MimeContent(stream),
                        ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                        ContentTransferEncoding = ContentEncoding.Base64,
                        FileName = Path.GetFileName(emailMessage.FileName.ElementAt(i)),
                    };
                    multipartContent.Add(attachment);
                }

                message.Body = multipartContent;
            }
            else
            {
                message.Body = new TextPart(TextFormat.Html)
                {
                    Text = emailMessage.Content,
                };
            }

            await this.SendItemAsync(message, traceProps).ConfigureAwait(false);
        }

        /// <summary>
        /// Sends the asynchronous.
        /// </summary>
        /// <param name="emailMessage">The EmailMessage.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task SendMeetingInviteAsync(EmailMessage emailMessage)
        {
            if (emailMessage == null)
            {
                throw new ArgumentNullException(nameof(emailMessage));
            }

            string recipients = string.Join(",", emailMessage.ToAddresses.Select(r => r.Address).ToList());

            var traceProps = new Dictionary<string, string>();
            traceProps["OperationName"] = this.GetType().FullName;
            traceProps["Recipients"] = recipients;

            var trimmedAddresses = emailMessage.ToAddresses.Where(a => !string.IsNullOrEmpty(a.Address)).ToArray();
            emailMessage.ToAddresses = trimmedAddresses;

            if (!emailMessage.ToAddresses.Any())
            {
                ArgumentNullException ex = new ArgumentNullException("ToAddresses", "Email message is missing 'To' addresses.");
                this.logger.WriteException(ex, traceProps);
                return;
            }

            var message = new MimeMessage();
            message.To.AddRange(emailMessage.ToAddresses.Select(x => new MailboxAddress(x.Name, x.Address)));
            if (emailMessage.CcAddresses != null && emailMessage.CcAddresses.Any())
            {
                message.Cc.AddRange(emailMessage.CcAddresses.Select(x => new MailboxAddress(x.Name, x.Address)));
            }

            message.From.AddRange(emailMessage.FromAddresses.Select(x => new MailboxAddress(this.mailConfiguration.FromAddressDisplayName, x.Address)));

            message.Subject = emailMessage.Subject;
            message.Importance = (MimeKit.MessageImportance)emailMessage.Importance;

            var ical = new TextPart("calendar")
            {
                ContentTransferEncoding = ContentEncoding.Base64,
                Text = emailMessage.Content,
            };

            ical.ContentType.Parameters.Add("method", "REQUEST");

            Multipart multipart = new Multipart("mixed");
            multipart.Add(ical);

            IList<string> fileNames = emailMessage.FileName?.ToList();
            IList<string> fileContents = emailMessage.FileContent?.ToList();
            int i = 0;
            if (fileNames != null && fileContents != null && fileNames.Any() && fileContents.Count == fileNames.Count)
            {
                foreach (var fileName in fileNames)
                {
                    string content = fileContents.ElementAt(i++);
                    if (!string.IsNullOrEmpty(content))
                    {
                        Stream stream = new MemoryStream(Convert.FromBase64String(content));
                        var indx = fileName.LastIndexOf('.') + 1;
                        var fileType = fileName.Substring(indx, fileName.Length - indx);
                        MimePart attachment = new MimePart("mixed", fileType)
                        {
                            FileName = fileName,
                            Content = new MimeContent(stream, ContentEncoding.Default),
                            ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                            ContentTransferEncoding = ContentEncoding.Base64,
                        };
                        multipart.Add(attachment);
                    }
                }
            }

            message.Body = multipart;
            await this.SendItemAsync(message, traceProps).ConfigureAwait(false);
        }

        private async Task SendItemAsync(MimeMessage message, Dictionary<string, string> traceProps)
        {
            IDSSmtpClient client = null;
            string status = "Fail";

            DateTimeOffset startTime;
            Stopwatch timer = new Stopwatch();
            try
            {
                client = await this.clientPool.GetClient(traceProps).ConfigureAwait(false);

                startTime = DateTimeOffset.Now;
                timer.Start();
                try
                {
                    await client.SendAsync(message, traceProps).ConfigureAwait(false);
                }
                catch (ServiceNotConnectedException svcEx)
                {
                    string description = $"SmtpClient not connected: {svcEx.Source}";
                    string eventMsg = "Closed connection, requesting new SmtpClient.";

                    this.logger.WriteException(svcEx, traceProps);
                    this.logger.TraceInformation(eventMsg, traceProps);

                    client.Refresh(traceProps);
                    await client.SendAsync(message, traceProps).ConfigureAwait(false);
                }
                catch (IOException ioex)
                {
                    string description = $"No response from MX endpoint: {ioex.Source}";
                    string eventMsg = "Socket failure, requesting new SmtpClient.";
                    traceProps["Error Description"] = description;
                    this.logger.WriteException(ioex, traceProps);
                    this.logger.TraceInformation(eventMsg, traceProps);

                    client.Refresh(traceProps);
                    await client.SendAsync(message, traceProps).ConfigureAwait(false);
                }
                catch (SmtpCommandException ex)
                {
                    this.logger.WriteException(ex, traceProps);
                    this.logger.TraceInformation($"SmtpCommandException with message {ex.Message} has been handled ", traceProps);
                    client.Refresh(traceProps);
                    await client.SendAsync(message, traceProps).ConfigureAwait(false);
                }
                catch (SocketException ex)
                {
                    this.logger.WriteException(ex, traceProps);
                    this.logger.TraceInformation($"SocketException with message {ex.Message} has been handled ", traceProps);
                    client.Refresh(traceProps);
                    await client.SendAsync(message, traceProps).ConfigureAwait(false);
                }

                timer.Stop();
                status = "Success";
            }
            catch (SmtpProtocolException ex)
            {
                this.logger.WriteException(ex, traceProps);
                throw;
            }
            catch (SmtpCommandException ex)
            {
                string msg = string.Empty;
                switch (ex.ErrorCode)
                {
                    case SmtpErrorCode.RecipientNotAccepted:
                        msg = $"Recipient not accepted: {ex.Mailbox?.Address}";
                        break;
                    case SmtpErrorCode.SenderNotAccepted:
                        msg = $"Sender not accepted: {ex.Mailbox?.Address}";
                        break;
                    case SmtpErrorCode.MessageNotAccepted:
                        msg = "Message not accepted.";
                        break;
                }

                traceProps["ErrorMessage"] = msg;
                this.logger.WriteException(ex, traceProps);
                throw;
            }
            catch (Exception ex)
            {
                this.logger.WriteException(ex, traceProps);
                throw;
            }
            finally
            {
                if (client != null)
                {
                    await this.clientPool.ReturnClient(client, traceProps).ConfigureAwait(false);
                }

                traceProps["Status"] = status;
                traceProps["EndPoint"] = this.clientPool.EndPoint;
                var metrics = new Dictionary<string, double>
                {
                    { "Duration", timer.Elapsed.TotalMilliseconds },
                };
                this.logger.WriteCustomEvent("DirectSendMailService_SendMail", traceProps, metrics);

                this.logger.WriteMetric("DirectSendMailService_SendMailCount", 1, traceProps);
            }
        }
    }
}
