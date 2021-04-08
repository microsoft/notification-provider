// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.FunctionalTests
{
    using Newtonsoft.Json;
    using NotificationService.Contracts;
    using NotificationService.Contracts.Models;
    using NUnit.Framework;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    public class EmailNotificationTemplateTest : BaseTests
    {
        [Test]
        public async Task SaveTemplateTest()
        {
            string TemplateID = "FunctionalTestTemplate";
            MailTemplate mailTemplate = new MailTemplate()
            {
                TemplateId = TemplateID,
                Description = "testing the template",
                TemplateType = "Text",
                Content = "<head>\r\n</head>\r\n<body>\r\n <p>Hi,</p>\r\n <p>{{MailSummary}}</p>\r\n <table border='1' style='border-collapse: collapse;' cellpadding='2' cellspacing='2'>\r\n <tbody>\r\n <tr><td>App Name</td> <td>{{appname}}</td> </tr>\r\n <tr><td>Content</td> <td>{{Content}}</td></td></tr>\r\n</body>"
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
                        await DeleteTemplateTest(mailTemplate.TemplateId);
                    }
                }
                else
                {
                    Assert.Fail();
                }
            }
        }

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

        private async Task SendEmailTemplateTest(MailTemplate mailTemplate, string TemplateData)
        {
            var emailNotificationItems = new EmailNotificationItem[]
            {
                new EmailNotificationItem() {
                To = this.Configuration[FunctionalConstants.ToAddress],
                Subject = "Notification Functional Testing using Template",
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
                        EmailMessage emailMessage = await GetNotificationMessage(notificationResponse.NotificationId, httpClient);
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

        private async Task<EmailMessage> GetNotificationMessage(string notificationId, HttpClient httpClient)
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

    }
}
