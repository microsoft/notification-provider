// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.FunctionalTests
{
    using Newtonsoft.Json;
    using NotificationService.Contracts;
    using NUnit.Framework;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;

    public class EmailNotificationQueueTest : BaseTests
    {

        [Test]
        public async Task QueueEmailGetNotificationMessageGetNotificationReportTest()
        {
            var emailNotificationItems = new EmailNotificationItem[]
            {
                new EmailNotificationItem() {
                To = Configuration[FunctionalConstants.ToAddress],
                Subject = "Notification Functional Testing of Email Queue Endpoint",
                Body = "Hello world!"
                }
            };

            var stringContent = new StringContent(JsonConvert.SerializeObject(emailNotificationItems), Encoding.UTF8, FunctionalConstants.ContentType);
            string notificationQueueEndpoint = $"{this.Configuration[FunctionalConstants.NotificationHandlerUrl]}/v1/email/queue/{this.Configuration[FunctionalConstants.Application]}";
            using (HttpClient httpClient = new HttpClient())
            {
                string bearerToken = await this.tokenUtility.GetTokenAsync();
                if (bearerToken != null)
                {
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(FunctionalConstants.Bearer, bearerToken);

                    var response = await httpClient.PostAsync(notificationQueueEndpoint, stringContent).ConfigureAwait(false);

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
                        Assert.IsTrue(notificationResponse.Status == NotificationItemStatus.Queued);
                        var notificationId = notificationResponse.NotificationId;
                        EmailMessage emailMessage = await GetNotificationMessageTest(notificationId, httpClient);
                        if (emailMessage != null)
                        {
                            Assert.IsTrue(emailNotificationItems[0].Subject == emailMessage.Subject);
                            Assert.IsTrue(emailNotificationItems[0].Body == emailMessage.Body.Content);
                        }
                        else
                        {
                            Assert.Fail();
                        }

                        int retryCount = int.TryParse(Configuration[FunctionalConstants.RetryCount], out retryCount) ? retryCount : 2;
                        int delayTime = int.TryParse(Configuration[FunctionalConstants.DelayTimeInMilliSeconds], out delayTime) ? delayTime : 5000;


                        for (int i = 0; i < retryCount; i++) {
                            NotificationReportResponse notificationReportResponse = await GetNotificationReportTest(notificationId, httpClient);
                            if (notificationReportResponse != null)
                            {
                                NotificationItemStatus notificationItemStatus = Enum.TryParse<NotificationItemStatus>(notificationReportResponse.Status, out notificationItemStatus) ? notificationItemStatus : NotificationItemStatus.Queued;
                                switch (notificationItemStatus) {
                                    case NotificationItemStatus.Failed:
                                    case NotificationItemStatus.FakeMail:
                                    case NotificationItemStatus.Invalid:
                                        {
                                            Assert.Fail();
                                            break;
                                        }
                                    case NotificationItemStatus.Sent:
                                        {
                                            Assert.Pass();
                                            break;
                                        }
                                    case NotificationItemStatus.Queued:
                                    case NotificationItemStatus.Processing:
                                    case NotificationItemStatus.Retrying:
                                        {
                                            if (i == retryCount - 1)
                                            {
                                                Assert.Fail();
                                                break;
                                            }
                                            await Task.Delay(delayTime);
                                            continue;
                                        }
                                }
                            }
                            else
                            {
                                Assert.Fail();
                            }
                        }

                    }
                }
                else
                {
                    Assert.Fail();
                }

            }
        }

        private async Task<NotificationReportResponse> GetNotificationReportTest(string notificationId, HttpClient httpClient)
        {
            NotificationReportResponse reportResponse = null;
         
            var reqcontent = "{\"applicationFilter\":[\"" + this.Configuration[FunctionalConstants.Application] + "\"], \"notificationIdsFilter\":[\"" + notificationId + "\"] }";
            var stringContent = new StringContent(reqcontent, Encoding.UTF8, FunctionalConstants.ContentType);

            string notificationReportEndpoint = $"{this.Configuration[FunctionalConstants.NotificationHandlerUrl]}/v1/report/notifications";
            var response = await httpClient.PostAsync(notificationReportEndpoint, stringContent).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                Assert.Fail();
            }
            else
            {
                var Result = response.Content.ReadAsStringAsync().Result;
                IList<NotificationReportResponse> reportResponses = JsonConvert.DeserializeObject<IList<NotificationReportResponse>>(Result);
                reportResponse = reportResponses.FirstOrDefault();
            }
            return reportResponse;
        }

        private async Task<EmailMessage> GetNotificationMessageTest(string notificationId, HttpClient httpClient)
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

        [Test]
        public async Task QueueEmailAttachmentGetNotificationTest()
        {
            var emailNotificationItems = new EmailNotificationItem[]
            {
                new EmailNotificationItem() {
                To = Configuration[FunctionalConstants.ToAddress],
                Subject = "Notification Functional Testing of Email Queue Endpoint with attachments",
                Body = "Hello world!",
                Attachments = new List<NotificationAttachment>()
                    {
                        new NotificationAttachment()
                        {
                            FileBase64 = "VEhpcyBpcyBhIHRlc3QgYXR0YWNobWVudCBmaWxlLg==",
                            FileName = "Test.txt",
                            IsInline = false,
                        },
                    },
                }
            };

            var stringContent = new StringContent(JsonConvert.SerializeObject(emailNotificationItems), Encoding.UTF8, FunctionalConstants.ContentType);
            string notificationQueueEndpoint = $"{this.Configuration[FunctionalConstants.NotificationHandlerUrl]}/v1/email/queue/{this.Configuration[FunctionalConstants.Application]}";
            using (HttpClient httpClient = new HttpClient())
            {
                string bearerToken = await this.tokenUtility.GetTokenAsync();
                if (bearerToken != null)
                {
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(FunctionalConstants.Bearer, bearerToken);

                    var response = await httpClient.PostAsync(notificationQueueEndpoint, stringContent).ConfigureAwait(false); ;

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
                        Assert.IsTrue(notificationResponse.Status == NotificationItemStatus.Queued);
                        var notificationId = notificationResponse.NotificationId;
                        EmailMessage emailMessage = await GetNotificationMessageTest(notificationId, httpClient);
                        if (emailMessage != null)
                        {
                            Assert.IsTrue(emailNotificationItems[0].Subject == emailMessage.Subject);
                            Assert.IsTrue(emailNotificationItems[0].Body == emailMessage.Body.Content);
                            Assert.IsTrue(emailNotificationItems[0].Attachments.FirstOrDefault().FileName == emailMessage.Attachments.FirstOrDefault().Name);
                            Assert.IsTrue(emailNotificationItems[0].Attachments.FirstOrDefault().FileBase64 == emailMessage.Attachments.FirstOrDefault().ContentBytes);
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

    }
}
