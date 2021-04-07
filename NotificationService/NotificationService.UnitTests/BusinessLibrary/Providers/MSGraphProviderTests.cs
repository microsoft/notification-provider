// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.UnitTests.BusinessLibrary.Providers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Options;
    using Moq;
    using Moq.Protected;
    using Newtonsoft.Json;
    using NotificationService.BusinessLibrary;
    using NotificationService.Common;
    using NotificationService.Contracts;
    using NotificationService.Contracts.Models.Graph.Invite;
    using NUnit.Framework;

    /// <summary>
    /// MSGraphProviderTests.
    /// </summary>
    public class MSGraphProviderTests : MSGraphProviderTestBase
    {
        /// <summary>
        /// RetrySetting Object.
        /// </summary>
        private readonly RetrySetting retrySetting = new RetrySetting
        {
            MaxRetries = 10,
            TransientRetryCount = 3,
        };

        /// <summary>
        /// Gets Test User Token value.
        /// </summary>
        public string TestToken => "TokenValue";

        /// <summary>
        /// Gets Test User Token value.
        /// </summary>
        public AuthenticationHeaderValue TestTokenHeader => new AuthenticationHeaderValue(ApplicationConstants.BearerAuthenticationScheme, this.TestToken);

        /// <summary>
        /// Initialization for the tests.
        /// </summary>
        [SetUp]
        public void Setup() => this.SetupTestBase();

        /// <summary>
        /// Processes the email request batch no active accounts.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Test]
        public async Task ProcessEmailRequestBatch_Response_Null()
        {
            var graphRequests = this.GetGraphRequest();

            Exception ex = null;
            var msGrpahProvider = new MSGraphProvider(this.MsGraphSetting, Options.Create(this.retrySetting), this.Logger, this.MockedHttpClient.Object);
            try
            {
                var result = await msGrpahProvider.ProcessEmailRequestBatch(this.TestTokenHeader, graphRequests).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                ex = e;
            }

            Assert.IsTrue(ex?.Message?.Contains("An error occurred while processing notification batch", StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Processes the email request batch no active accounts.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Test]
        public async Task ProcessEmailRequestBatch_CannotBeRetried()
        {
            var graphRequests = this.GetGraphRequest();

            var httpresponsemessage = this.GetEmailResponseMessage(graphRequests, "ErrorSubmissionQuotaExceeded", "Quota Exceeded", HttpStatusCode.TooManyRequests);

            var handlerMock = new Mock<HttpMessageHandler>();
            _ = handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>())
               .ReturnsAsync(httpresponsemessage);

            var httpClient = new HttpClient(handlerMock.Object);
            Exception ex = null;
            var msGrpahProvider = new MSGraphProvider(this.MsGraphSetting, Options.Create(this.retrySetting), this.Logger, httpClient);
            try
            {
                var result = await msGrpahProvider.ProcessEmailRequestBatch(this.TestTokenHeader, graphRequests).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                ex = e;
            }

            Assert.IsTrue(ex?.Message?.Contains("An error occurred while processing notification batch", StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Processes the email request batch no active accounts.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Test]
        public async Task ProcessEmailRequestBatch_ErrorGettingToken_CannotBeRetried_2()
        {
            var graphRequests = this.GetGraphRequest();

            var httpresponsemessage = this.GetEmailResponseMessage(graphRequests, "ErrorSendAsDenied", string.Empty, HttpStatusCode.TooManyRequests);
            var handlerMock = new Mock<HttpMessageHandler>();
            _ = handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>())
               .ReturnsAsync(httpresponsemessage);

            var httpClient = new HttpClient(handlerMock.Object);
            Exception ex = null;
            var msGrpahProvider = new MSGraphProvider(this.MsGraphSetting, Options.Create(this.retrySetting), this.Logger, httpClient);
            try
            {
                var result = await msGrpahProvider.ProcessEmailRequestBatch(this.TestTokenHeader, graphRequests).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                ex = e;
            }

            Assert.IsTrue(ex?.Message?.Contains("An error occurred while processing notification batch", StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Processes the email request batch no active accounts.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Test]
        public async Task ProcessEmailRequestBatch_Success()
        {
            var graphRequests = this.GetGraphRequest();

            var httpresponsemessage = this.GetEmailResponseMessage(graphRequests, null, null, HttpStatusCode.Accepted);

            var handlerMock = new Mock<HttpMessageHandler>();
            _ = handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>())
               .ReturnsAsync(httpresponsemessage);

            var httpClient = new HttpClient(handlerMock.Object);

            var msGrpahProvider = new MSGraphProvider(this.MsGraphSetting, Options.Create(this.retrySetting), this.Logger, httpClient);
            var result = await msGrpahProvider.ProcessEmailRequestBatch(this.TestTokenHeader, graphRequests).ConfigureAwait(false);
            Assert.AreEqual(2, result.Where(x => x.Status == HttpStatusCode.Accepted).Count());
        }

        /// <summary>
        /// Test send invite requests which return null http response.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Test]
        public async Task ProcessSendInvite_Response_Null()
        {
            var notificationId = Guid.NewGuid().ToString();
            Exception ex = null;
            var msGrpahProvider = new MSGraphProvider(this.MsGraphSetting, Options.Create(this.retrySetting), this.Logger, this.MockedHttpClient.Object);
            try
            {
                var result = await msGrpahProvider.SendMeetingInvite(this.TestTokenHeader, this.GetInvitePayload(), notificationId).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                ex = e;
            }

            Assert.IsTrue(ex?.Message?.Contains($"An error occurred while sending notification id: {notificationId}"));
        }

        /// <summary>
        /// Test send invites which returns BadRequest.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Test]
        public async Task ProcessSendInvite_Response_BadRequest()
        {
            HttpResponseMessage resp = this.GetHttpResponseMessage(HttpStatusCode.BadRequest, null);
            var handlerMock = new Mock<HttpMessageHandler>();
            _ = handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>())
               .ReturnsAsync(resp);
            var httpClient = new HttpClient(handlerMock.Object);
            var notificationId = Guid.NewGuid().ToString();
            Exception ex = null;
            var msGrpahProvider = new MSGraphProvider(this.MsGraphSetting, Options.Create(this.retrySetting), this.Logger, httpClient);
            try
            {
                var result = await msGrpahProvider.SendMeetingInvite(this.TestTokenHeader, this.GetInvitePayload(), notificationId).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                ex = e;
            }

            Assert.IsTrue(ex?.Message?.Contains($"An error occurred while sending notification id: {notificationId}"));
        }

        /// <summary>
        /// Test send invites which returns BadRequest.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Test]
        public async Task ProcessSendInvite_Response_TooManyRequest()
        {
            HttpResponseMessage resp = this.GetHttpResponseMessage(HttpStatusCode.TooManyRequests, null);
            var handlerMock = new Mock<HttpMessageHandler>();
            _ = handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>())
               .ReturnsAsync(resp);
            var httpClient = new HttpClient(handlerMock.Object);
            var notificationId = Guid.NewGuid().ToString();
            var msGrpahProvider = new MSGraphProvider(this.MsGraphSetting, Options.Create(this.retrySetting), this.Logger, httpClient);
            var result = await msGrpahProvider.SendMeetingInvite(this.TestTokenHeader, this.GetInvitePayload(), notificationId).ConfigureAwait(false);
            Assert.AreEqual(result.StatusCode, HttpStatusCode.TooManyRequests);
        }

        /// <summary>
        /// Test send invites which returns RequestTimeout.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Test]
        public async Task ProcessSendInvite_RequestTimeout()
        {
            HttpResponseMessage resp = this.GetHttpResponseMessage(HttpStatusCode.RequestTimeout, null);
            var handlerMock = new Mock<HttpMessageHandler>();
            _ = handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>())
               .ReturnsAsync(resp);
            var httpClient = new HttpClient(handlerMock.Object);
            var notificationId = Guid.NewGuid().ToString();
            var msGrpahProvider = new MSGraphProvider(this.MsGraphSetting, Options.Create(this.retrySetting), this.Logger, httpClient);
            var result = await msGrpahProvider.SendMeetingInvite(this.TestTokenHeader, this.GetInvitePayload(), notificationId).ConfigureAwait(false);
            Assert.AreEqual(result.StatusCode, HttpStatusCode.RequestTimeout);
        }

        /// <summary>
        /// Test Send invites which returns RequestTimeout.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Test]
        public async Task ProcessSendInvite_Success()
        {
            var id = Guid.NewGuid().ToString();
            HttpResponseMessage resp = this.GetHttpResponseMessage(HttpStatusCode.Accepted, id);
            var handlerMock = new Mock<HttpMessageHandler>();
            _ = handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>())
               .ReturnsAsync(resp);
            var httpClient = new HttpClient(handlerMock.Object);
            var notificationId = Guid.NewGuid().ToString();
            var msGrpahProvider = new MSGraphProvider(this.MsGraphSetting, Options.Create(this.retrySetting), this.Logger, httpClient);
            var result = await msGrpahProvider.SendMeetingInvite(this.TestTokenHeader, this.GetInvitePayload(), notificationId).ConfigureAwait(false);
            Assert.AreEqual(result.StatusCode, HttpStatusCode.Accepted);
            var res = JsonConvert.DeserializeObject<InviteResponse>(result.Result);
            Assert.AreEqual(res.EventId, id);
        }

        /// <summary>
        /// Test Update invite requests which return null http response.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Test]
        public async Task ProcessUpdateInvite_Response_Null()
        {
            var eventId = Guid.NewGuid().ToString();
            var notificationId = Guid.NewGuid().ToString();
            Exception ex = null;
            var msGrpahProvider = new MSGraphProvider(this.MsGraphSetting, Options.Create(this.retrySetting), this.Logger, this.MockedHttpClient.Object);
            try
            {
                var result = await msGrpahProvider.UpdateMeetingInvite(this.TestTokenHeader, this.GetInvitePayload(), notificationId, eventId).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                ex = e;
            }

            Assert.IsTrue(ex?.Message?.Contains($"An error occurred while sending notification id: {notificationId}"));
        }

        /// <summary>
        /// Test update invites which returns BadRequest.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Test]
        public async Task ProcessUpdateInvite_Response_BadRequest()
        {
            var eventId = Guid.NewGuid().ToString();
            HttpResponseMessage resp = this.GetHttpResponseMessage(HttpStatusCode.BadRequest, null);
            var handlerMock = new Mock<HttpMessageHandler>();
            _ = handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>())
               .ReturnsAsync(resp);
            var httpClient = new HttpClient(handlerMock.Object);
            var notificationId = Guid.NewGuid().ToString();
            Exception ex = null;
            var msGrpahProvider = new MSGraphProvider(this.MsGraphSetting, Options.Create(this.retrySetting), this.Logger, httpClient);
            try
            {
                var result = await msGrpahProvider.UpdateMeetingInvite(this.TestTokenHeader, this.GetInvitePayload(), notificationId, eventId).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                ex = e;
            }

            Assert.IsTrue(ex?.Message?.Contains($"An error occurred while sending notification id: {notificationId}"));
        }

        /// <summary>
        /// Test update invites which returns BadRequest.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Test]
        public async Task ProcessUpdateInvite_Response_TooManyRequest()
        {
            var eventId = Guid.NewGuid().ToString();
            HttpResponseMessage resp = this.GetHttpResponseMessage(HttpStatusCode.TooManyRequests, null);
            var handlerMock = new Mock<HttpMessageHandler>();
            _ = handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>())
               .ReturnsAsync(resp);
            var httpClient = new HttpClient(handlerMock.Object);
            var notificationId = Guid.NewGuid().ToString();
            var msGrpahProvider = new MSGraphProvider(this.MsGraphSetting, Options.Create(this.retrySetting), this.Logger, httpClient);
            var result = await msGrpahProvider.UpdateMeetingInvite(this.TestTokenHeader, this.GetInvitePayload(), notificationId, eventId).ConfigureAwait(false);
            Assert.AreEqual(result.StatusCode, HttpStatusCode.TooManyRequests);
        }

        /// <summary>
        /// Test update invites which returns RequestTimeout.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Test]
        public async Task ProcessUpdateInvite_RequestTimeout()
        {
            var eventId = Guid.NewGuid().ToString();
            HttpResponseMessage resp = this.GetHttpResponseMessage(HttpStatusCode.RequestTimeout, null);
            var handlerMock = new Mock<HttpMessageHandler>();
            _ = handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>())
               .ReturnsAsync(resp);
            var httpClient = new HttpClient(handlerMock.Object);
            var notificationId = Guid.NewGuid().ToString();
            var msGrpahProvider = new MSGraphProvider(this.MsGraphSetting, Options.Create(this.retrySetting), this.Logger, httpClient);
            var result = await msGrpahProvider.UpdateMeetingInvite(this.TestTokenHeader, this.GetInvitePayload(), notificationId, eventId).ConfigureAwait(false);
            Assert.AreEqual(result.StatusCode, HttpStatusCode.RequestTimeout);
        }

        /// <summary>
        /// Test update invites with success.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Test]
        public async Task ProcessUpdateInvite_Success()
        {
            var eventId = Guid.NewGuid().ToString();
            var id = Guid.NewGuid().ToString();
            HttpResponseMessage resp = this.GetHttpResponseMessage(HttpStatusCode.Accepted, id);
            var handlerMock = new Mock<HttpMessageHandler>();
            _ = handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>())
               .ReturnsAsync(resp);
            var httpClient = new HttpClient(handlerMock.Object);
            var notificationId = Guid.NewGuid().ToString();
            var msGrpahProvider = new MSGraphProvider(this.MsGraphSetting, Options.Create(this.retrySetting), this.Logger, httpClient);
            var result = await msGrpahProvider.UpdateMeetingInvite(this.TestTokenHeader, this.GetInvitePayload(), notificationId, eventId).ConfigureAwait(false);
            Assert.AreEqual(result.StatusCode, HttpStatusCode.Accepted);
            var res = JsonConvert.DeserializeObject<InviteResponse>(result.Result);
            Assert.AreEqual(res.EventId, id);
        }

        /// <summary>
        /// Test Delete invites with success.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Test]
        public async Task ProcessDeleteInvite_Success()
        {
            var eventId = Guid.NewGuid().ToString();
            var id = Guid.NewGuid().ToString();
            HttpResponseMessage resp = this.GetHttpResponseMessage(HttpStatusCode.NoContent, id);
            var handlerMock = new Mock<HttpMessageHandler>();
            _ = handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>())
               .ReturnsAsync(resp);
            var httpClient = new HttpClient(handlerMock.Object);
            var notificationId = Guid.NewGuid().ToString();
            var msGrpahProvider = new MSGraphProvider(this.MsGraphSetting, Options.Create(this.retrySetting), this.Logger, httpClient);
            var result = await msGrpahProvider.DeleteMeetingInvite(this.TestTokenHeader, notificationId, eventId).ConfigureAwait(false);
            Assert.AreEqual(result.StatusCode, HttpStatusCode.NoContent);
        }

        /// <summary>
        /// Test delete invites which returns RequestTimeout.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Test]
        public async Task ProcessDeleteInvite_RequestTimeout()
        {
            var eventId = Guid.NewGuid().ToString();
            var id = Guid.NewGuid().ToString();
            HttpResponseMessage resp = this.GetHttpResponseMessage(HttpStatusCode.RequestTimeout, id);
            var handlerMock = new Mock<HttpMessageHandler>();
            _ = handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>())
               .ReturnsAsync(resp);
            var httpClient = new HttpClient(handlerMock.Object);
            var notificationId = Guid.NewGuid().ToString();
            var msGrpahProvider = new MSGraphProvider(this.MsGraphSetting, Options.Create(this.retrySetting), this.Logger, httpClient);
            var result = await msGrpahProvider.DeleteMeetingInvite(this.TestTokenHeader, notificationId, eventId).ConfigureAwait(false);
            Assert.AreEqual(result.StatusCode, HttpStatusCode.RequestTimeout);
        }

        /// <summary>
        /// Test delete invites which returns BadRequest.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Test]
        public async Task ProcessDeleteInvite_BadRequest()
        {
            var eventId = Guid.NewGuid().ToString();
            var id = Guid.NewGuid().ToString();
            HttpResponseMessage resp = this.GetHttpResponseMessage(HttpStatusCode.BadRequest, id);
            var handlerMock = new Mock<HttpMessageHandler>();
            _ = handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>())
               .ReturnsAsync(resp);
            var httpClient = new HttpClient(handlerMock.Object);
            var notificationId = Guid.NewGuid().ToString();
            var msGrpahProvider = new MSGraphProvider(this.MsGraphSetting, Options.Create(this.retrySetting), this.Logger, httpClient);
            Exception ex = null;
            try
            {
                var result = await msGrpahProvider.DeleteMeetingInvite(this.TestTokenHeader, notificationId, eventId).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                ex = e;
            }

            Assert.IsTrue(ex?.Message?.Contains($"An error occurred while sending notification id: {notificationId}"));
        }

        /// <summary>
        /// Test delete invites which returns TooManyRequests.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Test]
        public async Task ProcessDeleteInvite_TooManyRequest()
        {
            var eventId = Guid.NewGuid().ToString();
            var id = Guid.NewGuid().ToString();
            HttpResponseMessage resp = this.GetHttpResponseMessage(HttpStatusCode.TooManyRequests, id);
            var handlerMock = new Mock<HttpMessageHandler>();
            _ = handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>())
               .ReturnsAsync(resp);
            var httpClient = new HttpClient(handlerMock.Object);
            var notificationId = Guid.NewGuid().ToString();
            var msGrpahProvider = new MSGraphProvider(this.MsGraphSetting, Options.Create(this.retrySetting), this.Logger, httpClient);
            var result = await msGrpahProvider.DeleteMeetingInvite(this.TestTokenHeader, notificationId, eventId).ConfigureAwait(false);
            Assert.AreEqual(result.StatusCode, HttpStatusCode.TooManyRequests);
        }

        /// <summary>
        /// Test delete invites which returns TooManyRequests.
        /// </summary>
        [Test]
        public void ProcessSendInviteAttachments_Success()
        {
            var eventId = Guid.NewGuid().ToString();
            var id = Guid.NewGuid().ToString();
            HttpResponseMessage resp = this.GetHttpResponseMessage(HttpStatusCode.Accepted, null);
            var handlerMock = new Mock<HttpMessageHandler>();
            _ = handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>())
               .ReturnsAsync(resp);
            var httpClient = new HttpClient(handlerMock.Object);
            var notificationId = Guid.NewGuid().ToString();
            var msGrpahProvider = new MSGraphProvider(this.MsGraphSetting, Options.Create(this.retrySetting), this.Logger, httpClient);
            var result = msGrpahProvider.SendMeetingInviteAttachments(this.TestTokenHeader, this.GetAttachments(), notificationId, eventId);
            Assert.GreaterOrEqual(2, result.Count);
        }

        private List<FileAttachment> GetAttachments()
        {
            return new List<FileAttachment>()
            {
                new FileAttachment()
                {
                    ContentBytes = "base64bWFjIGFuZCBjaGVlc2UgdG9kYXk=",
                    Name = "menu.txt",
                    IsInline = false,
                },
                new FileAttachment()
                {
                    ContentBytes = "VEhpcyBpcyBhIHRlc3QgYXR0YWNobWVudCBmaWxlLg==",
                    Name = "Test.txt",
                    IsInline = false,
                },
            };
        }

        private InvitePayload GetInvitePayload()
        {
            return new InvitePayload();
        }

        private HttpResponseMessage GetHttpResponseMessage(HttpStatusCode httpStatusCode, string eventId)
        {
            return new HttpResponseMessage(httpStatusCode)
            {
                Content = new StringContent(JsonConvert.SerializeObject(string.IsNullOrEmpty(eventId) ? null : new InviteResponse { EventId = eventId })),
            };
        }

        private GraphBatchRequest GetGraphRequest()
        {
            return new GraphBatchRequest
            {
                Requests = new List<GraphRequest>
                {
                    new GraphRequest
                    {
                        Id = Guid.NewGuid().ToString(),
                        Body = "Test Body",
                    },
                    new GraphRequest
                    {
                        Id = Guid.NewGuid().ToString(),
                        Body = "Test Body",
                    },
                },
            };
        }

        private HttpResponseMessage GetEmailResponseMessage(GraphBatchRequest request, string errorCode, string message, HttpStatusCode statusCode)
        {
            var graphResponses = request.Requests.Select(e => new GraphResponse
            {
                Status = statusCode,
                Body = new GraphResponseBody { Error = new GraphResponseError { Code = errorCode, Messsage = message } },
                Id = e.Id,
            });

            var graphBatchResponse = new GraphBatchResponse
            {
                Responses = graphResponses.ToList(),
            };

            return new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(JsonConvert.SerializeObject(graphBatchResponse)),
            };
        }
    }
}
