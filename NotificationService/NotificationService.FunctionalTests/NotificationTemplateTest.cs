// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.FunctionalTests
{
    using Newtonsoft.Json;
    using NotificationService.Contracts;
    using NotificationService.Contracts.Models;
    using NotificationService.Contracts.Models.Reports;
    using NUnit.Framework;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    public class NotificationTemplateTest : BaseTests
    {
        /// <summary>
        /// Test to save template, send email and meeting invite using that template and delete the template.
        /// </summary>
        [Test]
        public async Task SaveTemplateSendEmailSendInviteDeleteTemplateTest()
        {
            string TemplateID = "FunctionalTestTemplate";
            MailTemplate mailTemplate = new MailTemplate()
            {
                TemplateId = TemplateID,
                Description = "testing the template",
                TemplateType = "Text",
                Content = "<html><body><p>Hi {{MailSummary}}</p><table border='1'><tbody><tr><td>AppName</td><td>{{appname}}</td></tr><tr><td>Content</td><td>{{Content}}</td></tr></tbody></table></body></html>"
            };

            string TemplateData = "{\"{{MailSummary}}\":\"This is a functional testing scenario with templates\",\"{{appname}}\":\"Test App\",\"{{Content}}\":\"Test content\"}";

            var stringContent = new StringContent(JsonConvert.SerializeObject(mailTemplate), Encoding.UTF8, FunctionalConstants.ContentType);
            string templateSaveEndpoint = $"{this.Configuration[FunctionalConstants.NotificationHandlerUrl]}/v1/email/mailTemplate/{this.Configuration[FunctionalConstants.Application]}";
            using (HttpClient httpClient = new HttpClient())
            {
                string bearerToken = await this.tokenUtility.GetTokenAsync();
                if (bearerToken != null)
                {
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(FunctionalConstants.Bearer, bearerToken);

                    var response = await httpClient.PostAsync(templateSaveEndpoint, stringContent).ConfigureAwait(false);

                    if (!response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        Assert.Fail();
                    }
                    else
                    {
                        var Result = response.Content.ReadAsStringAsync().Result;
                        Assert.IsTrue(Boolean.Parse(Result) == true);
                        await SendEmailTemplateTest(mailTemplate, TemplateData);
                        await SendMeetingInviteTemplateTest(mailTemplate, TemplateData);
                        await DeleteTemplateTest(mailTemplate.TemplateId);
                    }
                }
                else
                {
                    Assert.Fail();
                }
            }
        }

        /// <summary>
        /// Deletes Template
        /// </summary>
        /// <param name="TemplateId"> TemplateId .</param>
        public async Task DeleteTemplateTest(string TemplateId)
        {

            string templateDeleteEndpoint = $"{this.Configuration[FunctionalConstants.NotificationHandlerUrl]}/v1/email/deleteTemplate/{this.Configuration[FunctionalConstants.Application]}/{TemplateId}";
            using (HttpClient httpClient = new HttpClient())
            {
                string bearerToken = await this.tokenUtility.GetTokenAsync();
                if (bearerToken != null)
                {
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(FunctionalConstants.Bearer, bearerToken);

                    var response = await httpClient.PostAsync(templateDeleteEndpoint, null).ConfigureAwait(false);

                    if (!response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        Assert.Fail();
                    }
                    else
                    {
                        var Result = response.Content.ReadAsStringAsync().Result;
                        Assert.IsTrue(Boolean.Parse(Result) == true);
                    }
                }
                else
                {
                    Assert.Fail();
                }
            }
        }

        /// <summary>
        /// sends email using template
        /// </summary>
        /// <param name="mailTemplate"> template .</param>
        /// <param name="TemplateData">template  data params .</param>
        private async Task SendEmailTemplateTest(MailTemplate mailTemplate, string TemplateData)
        {
            var emailNotificationItems = new EmailNotificationItem[]
            {
                new EmailNotificationItem() {
                To = this.Configuration[FunctionalConstants.ToAddress],
                Subject = "Email Notification Functional Testing of Template through send endpoint",
                Priority = NotificationPriority.Low,
                TemplateId = mailTemplate.TemplateId,
                TemplateData = TemplateData
                }
            };

            var stringContent = new StringContent(JsonConvert.SerializeObject(emailNotificationItems), Encoding.UTF8, FunctionalConstants.ContentType);
            string notificationServiceEndpoint = $"{this.Configuration[FunctionalConstants.NotificationServiceUrl]}/v1/email/send/{this.Configuration[FunctionalConstants.Application]}";
            using (HttpClient httpClient = new HttpClient())
            {
                string bearerToken = await this.tokenUtility.GetTokenAsync();
                if (bearerToken != null)
                {
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(FunctionalConstants.Bearer, bearerToken);

                    var response = await httpClient.PostAsync(notificationServiceEndpoint, stringContent).ConfigureAwait(false);

                    if (!response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        Assert.Fail();
                    }
                    else
                    {
                        var Result = response.Content.ReadAsStringAsync().Result;
                        var notificationResponses = JsonConvert.DeserializeObject<List<NotificationResponse>>(Result);
                        var notificationResponse = notificationResponses.FirstOrDefault();
                        Assert.IsTrue(notificationResponse.Status == NotificationItemStatus.Sent);
                        EmailMessage emailMessage = await GetEmailNotificationMessage(notificationResponse.NotificationId, httpClient);
                        if (emailMessage != null)
                        {
                            var templateBody = ConvertText(mailTemplate.Content, TemplateData);
                            Assert.IsTrue(emailNotificationItems[0].Subject == emailMessage.Subject);
                            Assert.IsTrue(templateBody == emailMessage.Body.Content);
                        }
                        else
                        {
                            Assert.Fail();
                        }
                    }
                }
                else
                {
                    Assert.Fail();
                }
            }
        }

        /// <summary>
        /// calls the notificationMessage report endpoint to get the message including body
        /// </summary>
        /// <param name="notificationId">notificationId .</param>
        /// <param name="httpClient">httpClient object .</param>
        /// <returns>EmailMessage corresponding to the notificationId</returns>
        private async Task<EmailMessage> GetEmailNotificationMessage(string notificationId, HttpClient httpClient)
        {
            EmailMessage emailMessage = null;
            string notificationMessageEndpoint = $"{this.Configuration[FunctionalConstants.NotificationHandlerUrl]}/v1/report/notificationMessage/{this.Configuration[FunctionalConstants.Application]}/{notificationId}";
            var response = await httpClient.GetAsync(notificationMessageEndpoint).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
            }
            else
            {
                var Result = response.Content.ReadAsStringAsync().Result;
                emailMessage = JsonConvert.DeserializeObject<EmailMessage>(Result);
            }
            return emailMessage;
        }

        /// <summary>
        /// Converts text template.
        /// </summary>
        /// <param name="notificationTemplate">notificationTemplate .</param>
        /// <param name="notificationText">notificationText .</param>
        /// <returns>text type template.</returns>
        private string ConvertText(string notificationTemplate, string notificationText)
        {
            if (string.IsNullOrEmpty(notificationText))
            {
                return notificationTemplate;
            }

            Dictionary<string, string> tokens = null;
            tokens = JsonConvert.DeserializeObject<Dictionary<string, string>>(notificationText);

            foreach (KeyValuePair<string, string> tokenConfiguration in tokens)
            {
                if (Regex.IsMatch(notificationTemplate, tokenConfiguration.Key))
                {
                    notificationTemplate = Regex.Replace(notificationTemplate, tokenConfiguration.Key, tokenConfiguration.Value);
                }
            }

            return notificationTemplate;
        }

        /// <summary>
        /// sends invite using template
        /// </summary>
        /// <param name="mailTemplate"> template .</param>
        /// <param name="TemplateData">template  data params .</param>
        private async Task SendMeetingInviteTemplateTest(MailTemplate mailTemplate, string TemplateData)
        {
            var date = DateTime.UtcNow;
            var meetingInviteItems = new MeetingNotificationItem[]
            {
                new MeetingNotificationItem() {
                    From = this.Configuration[FunctionalConstants.ToAddress],
                    RequiredAttendees = this.Configuration[FunctionalConstants.ToAddress],
                    Subject = "Meeting Invite Functional Testing of Template through send endpoint",
                    Start = date,
                    End = date.AddHours(1),
                    Priority = NotificationPriority.Low,
                    TemplateId = mailTemplate.TemplateId,
                    TemplateData = TemplateData
                }
            };

            var stringContent = new StringContent(JsonConvert.SerializeObject(meetingInviteItems), Encoding.UTF8, FunctionalConstants.ContentType);
            string notificationServiceEndpoint = $"{this.Configuration[FunctionalConstants.NotificationServiceUrl]}/v1/meetinginvite/send/{this.Configuration[FunctionalConstants.Application]}";
            using (HttpClient httpClient = new HttpClient())
            {
                string bearerToken = await this.tokenUtility.GetTokenAsync();
                if (bearerToken != null)
                {
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(FunctionalConstants.Bearer, bearerToken);

                    var response = await httpClient.PostAsync(notificationServiceEndpoint, stringContent).ConfigureAwait(false);

                    if (!response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        Assert.Fail();
                    }
                    else
                    {
                        var Result = response.Content.ReadAsStringAsync().Result;
                        var notificationResponses = JsonConvert.DeserializeObject<List<NotificationResponse>>(Result);
                        var notificationResponse = notificationResponses.FirstOrDefault();
                        Assert.IsTrue(notificationResponse.Status == NotificationItemStatus.Sent);
                        MeetingInviteMessage inviteMessage = await GetMeetingNotificationMessage(notificationResponse.NotificationId, httpClient);
                        if (inviteMessage != null)
                        {
                            var templateBody = ConvertText(mailTemplate.Content, TemplateData);
                            Assert.IsTrue(meetingInviteItems[0].Subject == inviteMessage.Subject);
                            Assert.IsTrue(templateBody == inviteMessage.Body);
                        }
                        else
                        {
                            Assert.Fail();
                        }
                    }
                }
                else
                {
                    Assert.Fail();
                }
            }
        }

        /// <summary>
        /// calls the meetingMessage report endpoint to get the message including body
        /// </summary>
        /// <param name="notificationId">notificationId .</param>
        /// <param name="httpClient">httpClient object .</param>
        /// <returns>MeetingInviteMessage corresponding to the notificationId</returns>
        private async Task<MeetingInviteMessage> GetMeetingNotificationMessage(string notificationId, HttpClient httpClient)
        {
            MeetingInviteMessage inviteMessage = null;
            string notificationMessageEndpoint = $"{this.Configuration[FunctionalConstants.NotificationHandlerUrl]}/v1/report/meetingMessage/{this.Configuration[FunctionalConstants.Application]}/{notificationId}";
            var response = await httpClient.GetAsync(notificationMessageEndpoint).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
            }
            else
            {
                var Result = response.Content.ReadAsStringAsync().Result;
                inviteMessage = JsonConvert.DeserializeObject<MeetingInviteMessage>(Result);
            }
            return inviteMessage;
        }

    }
}
