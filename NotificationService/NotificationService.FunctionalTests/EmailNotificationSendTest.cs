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

    public class EmailNotificationSendTest : BaseTests
    {
        [Test]
        public async Task SendEmailTest()
        {
            var emailNotificationItems = new EmailNotificationItem[]
            {
                new EmailNotificationItem() {
                To = Configuration[FunctionalConstants.ToAddress],
                Subject = "Notification Functional Testing of Email Send Endpoint",
                Body = "Hello world!"
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

                    var response = await httpClient.PostAsync(notificationServiceEndpoint, stringContent).ConfigureAwait(false); ;

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
