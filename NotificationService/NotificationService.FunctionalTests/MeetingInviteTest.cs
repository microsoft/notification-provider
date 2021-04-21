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
    using System.Threading.Tasks;

    public class MeetingInviteTest : BaseTests
    {
        [Test]
        /// <summary>
        /// Test meeting invite queue, meeting invite report and meeting invite message including body 
        /// </summary>
        public async Task MeetingInvitesQueueReportMessageTest()
        {
            var date = DateTime.UtcNow;
            var meetingInviteItems = new MeetingNotificationItem[]
            {
                new MeetingNotificationItem() {
                    From = this.Configuration[FunctionalConstants.ToAddress],
                    RequiredAttendees = this.Configuration[FunctionalConstants.ToAddress],
                    Subject = "Functional Testing of Meeting Invites Queue endpoint",
                    Body = "Lets meet!",
                    Start = date,
                    End = date.AddHours(1),
                    Priority = NotificationPriority.Normal
                }
            };

            var stringContent = new StringContent(JsonConvert.SerializeObject(meetingInviteItems), Encoding.UTF8, FunctionalConstants.ContentType);
            string meetingInviteQueueEndpoint = $"{this.Configuration[FunctionalConstants.NotificationHandlerUrl]}/v1/meetinginvite/queue/{this.Configuration[FunctionalConstants.Application]}";
            using (HttpClient httpClient = new HttpClient())
            {
                string bearerToken = await this.tokenUtility.GetTokenAsync();
                if (bearerToken != null)
                {
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(FunctionalConstants.Bearer, bearerToken);

                    var response = await httpClient.PostAsync(meetingInviteQueueEndpoint, stringContent).ConfigureAwait(false);

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
                        MeetingInviteMessage inviteMessage = await GetMeetingNotificationMessage(notificationId, httpClient);
                        if (inviteMessage != null)
                        {
                            Assert.IsTrue(meetingInviteItems[0].Subject == inviteMessage.Subject);
                            Assert.IsTrue(meetingInviteItems[0].Body == inviteMessage.Body);
                        }
                        else
                        {
                            Assert.Fail();
                        }

                        int retryCount = int.TryParse(Configuration[FunctionalConstants.RetryCount], out retryCount) ? retryCount : 2;
                        int delayTime = int.TryParse(Configuration[FunctionalConstants.DelayTimeInMilliSeconds], out delayTime) ? delayTime : 5000;


                        for (int i = 0; i < retryCount; i++)
                        {
                            MeetingInviteReportResponse inviteReportResponse = await GetMeetingInviteReportTest(notificationId, httpClient);
                            if (inviteReportResponse != null)
                            {
                                NotificationItemStatus notificationItemStatus = Enum.TryParse<NotificationItemStatus>(inviteReportResponse.Status, out notificationItemStatus) ? notificationItemStatus : NotificationItemStatus.Queued;
                                switch (notificationItemStatus)
                                {
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

        /// <summary>
        /// calls the meetingInvites report endpoint.
        /// </summary>
        /// <param name="notificationId">notificationId .</param>
        /// <param name="httpClient">httpClient object .</param>
        /// <returns>MeetingInviteReportResponse corresponding to the notificationId</returns>
        private async Task<MeetingInviteReportResponse> GetMeetingInviteReportTest(string notificationId, HttpClient httpClient)
        {
            MeetingInviteReportResponse reportResponse = null;

            var reqcontent = "{\"applicationFilter\":[\"" + this.Configuration[FunctionalConstants.Application] + "\"], \"notificationIdsFilter\":[\"" + notificationId + "\"] }";
            var stringContent = new StringContent(reqcontent, Encoding.UTF8, FunctionalConstants.ContentType);

            string inviteReportEndpoint = $"{this.Configuration[FunctionalConstants.NotificationHandlerUrl]}/v1/report/meetingInvites";
            var response = await httpClient.PostAsync(inviteReportEndpoint, stringContent).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                Assert.Fail();
            }
            else
            {
                var Result = response.Content.ReadAsStringAsync().Result;
                IList<MeetingInviteReportResponse> reportResponses = JsonConvert.DeserializeObject<IList<MeetingInviteReportResponse>>(Result);
                reportResponse = reportResponses.FirstOrDefault();
            }
            return reportResponse;
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
            string inviteMessageEndpoint = $"{this.Configuration[FunctionalConstants.NotificationHandlerUrl]}/v1/report/meetingMessage/{this.Configuration[FunctionalConstants.Application]}/{notificationId}";
            var response = await httpClient.GetAsync(inviteMessageEndpoint).ConfigureAwait(false);
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

        [Test]
        /// <summary>
        /// Test meeting invite with attachment through queue endpoint
        /// </summary>
        public async Task QueueMeetingInvitesWithAttachmentAndMeetingInviteReportTest()
        {
            var date = DateTime.UtcNow;
            var meetingInviteItems = new MeetingNotificationItem[]
            {
                new MeetingNotificationItem() {
                    From = this.Configuration[FunctionalConstants.ToAddress],
                    RequiredAttendees = this.Configuration[FunctionalConstants.ToAddress],
                    Subject = "Functional Testing of Meeting Invites Queue endpoint with attachments",
                    Body = "Lets meet!",
                    Start = date,
                    End = date.AddHours(1),
                    Priority = NotificationPriority.Normal,
                    Attachments = new NotificationAttachment[]
                    {
                        new NotificationAttachment()
                        {
                            FileBase64 = "VEhpcyBpcyBhIHRlc3QgYXR0YWNobWVudCBmaWxlLg==",
                            FileName = "Test.txt"
                        },
                    },
                }
            };

            var stringContent = new StringContent(JsonConvert.SerializeObject(meetingInviteItems), Encoding.UTF8, FunctionalConstants.ContentType);
            string meetingInviteQueueEndpoint = $"{this.Configuration[FunctionalConstants.NotificationHandlerUrl]}/v1/meetinginvite/queue/{this.Configuration[FunctionalConstants.Application]}";
            using (HttpClient httpClient = new HttpClient())
            {
                string bearerToken = await this.tokenUtility.GetTokenAsync();
                if (bearerToken != null)
                {
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(FunctionalConstants.Bearer, bearerToken);

                    var response = await httpClient.PostAsync(meetingInviteQueueEndpoint, stringContent).ConfigureAwait(false);

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
                        MeetingInviteMessage inviteMessage = await GetMeetingNotificationMessage(notificationId, httpClient);
                        if (inviteMessage != null)
                        {
                            Assert.IsTrue(meetingInviteItems[0].Subject == inviteMessage.Subject);
                            Assert.IsTrue(meetingInviteItems[0].Body == inviteMessage.Body);
                            Assert.IsTrue(meetingInviteItems[0].Attachments.FirstOrDefault().FileName == inviteMessage.Attachments.FirstOrDefault().Name);
                            Assert.IsTrue(meetingInviteItems[0].Attachments.FirstOrDefault().FileBase64 == inviteMessage.Attachments.FirstOrDefault().ContentBytes);
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

        [Test]
        /// <summary>
        /// Test meeting invite through send endpoint
        /// </summary>
        public async Task SendMeetingInvitesTest()
        {
            var date = DateTime.UtcNow;
            var meetingInviteItems = new MeetingNotificationItem[]
            {
                new MeetingNotificationItem() {
                    From = this.Configuration[FunctionalConstants.ToAddress],
                    RequiredAttendees = this.Configuration[FunctionalConstants.ToAddress],
                    Subject = "Functional Testing of Meeting Invites Send endpoint",
                    Body = "Lets meet!",
                    Start = date,
                    End = date.AddHours(1),
                    Priority = NotificationPriority.Low
                }
            };

            var stringContent = new StringContent(JsonConvert.SerializeObject(meetingInviteItems), Encoding.UTF8, FunctionalConstants.ContentType);
            string meetingInviteQueueEndpoint = $"{this.Configuration[FunctionalConstants.NotificationServiceUrl]}/v1/meetinginvite/send/{this.Configuration[FunctionalConstants.Application]}";
            using (HttpClient httpClient = new HttpClient())
            {
                string bearerToken = await this.tokenUtility.GetTokenAsync();
                if (bearerToken != null)
                {
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(FunctionalConstants.Bearer, bearerToken);

                    var response = await httpClient.PostAsync(meetingInviteQueueEndpoint, stringContent).ConfigureAwait(false);

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
