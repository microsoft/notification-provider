// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.FunctionalTests
{
    using Newtonsoft.Json;
    using NotificationService.Contracts;
    using NotificationService.Contracts.Entities;
    using NotificationService.Contracts.Models;
    using NUnit.Framework;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;

    public class MeetingInviteTest  : BaseTests
    {
        [Test]
        public async Task QueueMeetingInvitesWithAttachmentTest()
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

                    }
                }   
                else
                {
                    Assert.Fail();
                }

            }
        }

        [Test]
        public async Task SendMeetingInvitesWithAttachmentsTest()
        {
            var date = DateTime.UtcNow;
            var meetingInviteItems = new MeetingNotificationItem[]
            {
                new MeetingNotificationItem() {
                    From = this.Configuration[FunctionalConstants.ToAddress],
                    RequiredAttendees = this.Configuration[FunctionalConstants.ToAddress],
                    Subject = "Functional Testing of Meeting Invites Send endpoint with attachments",
                    Body = "Lets meet!",
                    Start = date,
                    End = date.AddHours(1),
                    Priority = NotificationPriority.Normal,
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
