// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

/*
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using NotificationService.BusinessLibrary;
using NotificationService.Common;
using NotificationService.Contracts;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace NotificationService.UnitTests.BusinessLibrary.Providers
{
    /// <summary>
    /// MSGraphProviderTests.
    /// </summary>
    public class MSGraphProviderTests : MSGraphProvideTestBase
    {
        /// <summary>
        /// Gets Test User Token value.
        /// </summary>
        public string TestToken => "TokenValue";

        /// <summary>
        /// Gets Test User Token value.
        /// </summary>
        public AuthenticationHeaderValue TestTokenHeader => new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", this.TestToken);

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
        public async Task ProcessEmailRequestBatch_NoActiveAccounts()
        {
            var applicationAccounts = new List<ApplicationAccounts>()
            {
                new ApplicationAccounts()
                {
                    ApplicationName = this.ApplicationName,
                    ValidAppIds = Guid.NewGuid().ToString(),
                    FromOverride = "TestFrom",
                    Accounts = new List<AccountCredential>()
                    {
                        new AccountCredential()
                        {
                            AccountName = "Test", IsEnabled = false, PrimaryPassword = "Test",
                        },
                    },
                },
            };

            var retrySetting = new RetrySetting
            {
                MaxRetries = 10,
                TransientRetryCount = 3,
            };

            Dictionary<string, string> testConfigValues = new Dictionary<string, string>()
            {
                { "ApplicationAccounts", JsonConvert.SerializeObject(applicationAccounts) },
                { "RetrySetting:MaxRetries", "10" },
                { "RetrySetting:TransientRetryCount", "3" },
            };

            this.Configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(testConfigValues)
                .Build();
            _ = this.TokenHelper
                .Setup(th => th.GetAccessTokenForSelectedAccount(It.IsAny<AccountCredential>()))
                .Returns(Task.FromResult(this.TestToken));

            _ = this.TokenHelper
                .Setup(th => th.GetAuthenticationHeaderFromToken(It.IsAny<string>()))
                .Returns(Task.FromResult(this.TestTokenHeader));

            var graphRequests = new GraphBatchRequest
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
            //var authenticationHeader = await this.TokenHelper.Object.GetAuthenticationHeaderValueForSelectedAccount(It.Is<AccountCredential>(a => a.AccountName.Equals("Test"))).ConfigureAwait(false);
            var msGrpahProvider = new MSGraphProvider(this.MsGraphSetting, Options.Create(retrySetting), this.Logger, this.MockedHttpClient.Object);
            var result = await msGrpahProvider.ProcessEmailRequestBatch(this.TestTokenHeader, graphRequests).ConfigureAwait(false);
            Assert.AreEqual(2, result.Where(x => x.Status == System.Net.HttpStatusCode.PreconditionFailed).Count());
            Assert.IsTrue(result.Any(x => x.Error.Contains("No active/valid email account exists for the application")));
        }

        /// <summary>
        /// Processes the email request batch no active accounts.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Test]
        public async Task ProcessEmailRequestBatch_ErrorGettingToken_ForallAccounts()
        {
            var applicationAccounts = new List<ApplicationAccounts>()
            {
                new ApplicationAccounts()
                {
                    ApplicationName = this.ApplicationName,
                    ValidAppIds = Guid.NewGuid().ToString(),
                    FromOverride = "TestFrom",
                    Accounts = new List<AccountCredential>()
                    {
                        new AccountCredential()
                        {
                            AccountName = "Test", IsEnabled = true, PrimaryPassword = "Test",
                        },
                    },
                },
            };

            var retrySetting = new RetrySetting
            {
                MaxRetries = 10,
                TransientRetryCount = 3,
            };

            Dictionary<string, string> testConfigValues = new Dictionary<string, string>()
            {
                { "ApplicationAccounts", JsonConvert.SerializeObject(applicationAccounts) },
                { "RetrySetting:MaxRetries", "10" },
                { "RetrySetting:TransientRetryCount", "3" },
            };

            this.Configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(testConfigValues)
                .Build();

            var graphRequests = new GraphBatchRequest
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
            //var authenticationHeader = await this.TokenHelper.Object.GetAuthenticationHeaderValueForSelectedAccount(It.Is<AccountCredential>(a => a.AccountName.Equals("Test"))).ConfigureAwait(false);

            var msGrpahProvider = new MSGraphProvider(this.MsGraphSetting, Options.Create(retrySetting), this.Logger, this.MockedHttpClient.Object);
            var result = await msGrpahProvider.ProcessEmailRequestBatch(this.TestTokenHeader, graphRequests).ConfigureAwait(false);
            Assert.AreEqual(2, result.Where(x => x.Status == System.Net.HttpStatusCode.PreconditionFailed).Count());
            Assert.IsTrue(result.Any(x => x.Error.Contains("No active/valid email account exists for the application")));
        }

        /// <summary>
        /// Processes the email request batch no active accounts.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Test]
        public async Task ProcessEmailRequestBatch_ErrorGettingToken_CannotBeRetried()
        {
            var applicationAccounts = new List<ApplicationAccounts>()
            {
                new ApplicationAccounts()
                {
                    ApplicationName = this.ApplicationName,
                    ValidAppIds = Guid.NewGuid().ToString(),
                    FromOverride = "TestFrom",
                    Accounts = new List<AccountCredential>()
                    {
                        new AccountCredential()
                        {
                            AccountName = "Test", IsEnabled = true, PrimaryPassword = "Test",
                        },
                        new AccountCredential()
                        {
                            AccountName = "Test2", IsEnabled = true, PrimaryPassword = "Test",
                        },
                    },
                },
            };

            var retrySetting = new RetrySetting
            {
                MaxRetries = 10,
                TransientRetryCount = 3,
            };

            Dictionary<string, string> testConfigValues = new Dictionary<string, string>()
            {
                { "ApplicationAccounts", JsonConvert.SerializeObject(applicationAccounts) },
                { "RetrySetting:MaxRetries", "10" },
                { "RetrySetting:TransientRetryCount", "3" },
            };

            this.Configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(testConfigValues)
                .Build();
            var notificationId1 = Guid.NewGuid().ToString();
            var notificationId2 = Guid.NewGuid().ToString();

            var graphRequests = new GraphBatchRequest
            {
                Requests = new List<GraphRequest>
                {
                    new GraphRequest
                    {
                        Id = notificationId1,
                        Body = "Test Body",
                    },
                    new GraphRequest
                    {
                        Id = notificationId1,
                        Body = "Test Body",
                    },
                },
            };
            string res = null;
            _ = this.TokenHelper.Setup(x => x.GetAccessTokenForSelectedAccount(It.Is<AccountCredential>(a => a.AccountName.Equals("Test")))).ReturnsAsync(res);
            _ = this.TokenHelper.Setup(x => x.GetAccessTokenForSelectedAccount(It.Is<AccountCredential>(a => a.AccountName.Equals("Test2")))).ReturnsAsync("token");
            _ = this.TokenHelper.Setup(x => x.GetAuthenticationHeaderFromToken(It.IsAny<string>())).ReturnsAsync(new System.Net.Http.Headers.AuthenticationHeaderValue("test"));

            var httpresponsemessage = new HttpResponseMessage
            {
                Content = new StringContent(JsonConvert.SerializeObject(new GraphBatchResponse
                {
                    Responses = new List<GraphResponse>
                    {
                        new GraphResponse
                        {
                                            Status = System.Net.HttpStatusCode.TooManyRequests,
                                            Body = new GraphResponseBody { Error = new GraphResponseError { Code = "ErrorSubmissionQuotaExceeded", Messsage = "Quota Exceeded" } },
                                            Id = notificationId1,
                        },
                        new GraphResponse
                        {
                                            Status = System.Net.HttpStatusCode.TooManyRequests,
                                            Body = new GraphResponseBody { Error = new GraphResponseError { Code = "ErrorSubmissionQuotaExceeded", Messsage = "Quota Exceeded" } },
                                            Id = notificationId2,
                        },
                    },
                })),
            };

            var handlerMock = new Mock<HttpMessageHandler>();
            _ = handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>())
               .ReturnsAsync(httpresponsemessage);

            var httpClient = new HttpClient(handlerMock.Object);

            //_ = this.MockedHttpClient.Setup(x => x.PostAsync(It.IsAny<string>(), It.IsAny<HttpContent>())).ReturnsAsync(httpresponsemessage);
            //var authenticationHeader = await this.TokenHelper.Object.GetAuthenticationHeaderValueForSelectedAccount(It.Is<AccountCredential>(a => a.AccountName.Equals("Test"))).ConfigureAwait(false);
            var msGrpahProvider = new MSGraphProvider(this.MsGraphSetting, Options.Create(retrySetting), this.Logger, httpClient);
            var result = await msGrpahProvider.ProcessEmailRequestBatch(this.TestTokenHeader, graphRequests).ConfigureAwait(false);
            Assert.AreEqual(2, result.Where(x => x.Status == System.Net.HttpStatusCode.TooManyRequests).Count());
            Assert.IsTrue(result.Any(x => x.Error.Contains("Quota Exceeded")));
        }

        /// <summary>
        /// Processes the email request batch no active accounts.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Test]
        public async Task ProcessEmailRequestBatch_ErrorGettingToken_CannotBeRetried_2()
        {
            var applicationAccounts = new List<ApplicationAccounts>()
            {
                new ApplicationAccounts()
                {
                    ApplicationName = this.ApplicationName,
                    ValidAppIds = Guid.NewGuid().ToString(),
                    FromOverride = "TestFrom",
                    Accounts = new List<AccountCredential>()
                    {
                        new AccountCredential()
                        {
                            AccountName = "Test", IsEnabled = true, PrimaryPassword = "Test",
                        },
                        new AccountCredential()
                        {
                            AccountName = "Test2", IsEnabled = true, PrimaryPassword = "Test",
                        },
                    },
                },
            };

            var retrySetting = new RetrySetting
            {
                MaxRetries = 10,
                TransientRetryCount = 3,
            };

            Dictionary<string, string> testConfigValues = new Dictionary<string, string>()
            {
                { "ApplicationAccounts", JsonConvert.SerializeObject(applicationAccounts) },
                { "RetrySetting:MaxRetries", "10" },
                { "RetrySetting:TransientRetryCount", "3" },
            };

            this.Configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(testConfigValues)
                .Build();
            var notificationId1 = Guid.NewGuid().ToString();
            var notificationId2 = Guid.NewGuid().ToString();

            var graphRequests = new GraphBatchRequest
            {
                Requests = new List<GraphRequest>
                {
                    new GraphRequest
                    {
                        Id = notificationId1,
                        Body = "Test Body",
                    },
                    new GraphRequest
                    {
                        Id = notificationId1,
                        Body = "Test Body",
                    },
                },
            };
            string res = null;
            _ = this.TokenHelper.Setup(x => x.GetAccessTokenForSelectedAccount(It.Is<AccountCredential>(a => a.AccountName.Equals("Test")))).ReturnsAsync("token");
            _ = this.TokenHelper.Setup(x => x.GetAccessTokenForSelectedAccount(It.Is<AccountCredential>(a => a.AccountName.Equals("Test2")))).ReturnsAsync("token");
            _ = this.TokenHelper.Setup(x => x.GetAuthenticationHeaderFromToken(It.IsAny<string>())).ReturnsAsync(new System.Net.Http.Headers.AuthenticationHeaderValue("test"));


            var httpresponsemessage = new HttpResponseMessage
            {
                Content = new StringContent(JsonConvert.SerializeObject(new GraphBatchResponse
                {
                    Responses = new List<GraphResponse>
                    {
                        new GraphResponse
                        {
                                            Status = System.Net.HttpStatusCode.TooManyRequests,
                                            Body = new GraphResponseBody { Error = new GraphResponseError { Code = "ErrorSendAsDenied", Messsage = "Quota Exceeded" } },
                                            Id = notificationId1,
                        },
                        new GraphResponse
                        {
                                            Status = System.Net.HttpStatusCode.TooManyRequests,
                                            Body = new GraphResponseBody { Error = new GraphResponseError { Code = "ErrorSendAsDenied", Messsage = "Quota Exceeded" } },
                                            Id = notificationId2,
                        },
                    },
                })),
            };

            var handlerMock = new Mock<HttpMessageHandler>();
            _ = handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>())
               .ReturnsAsync(httpresponsemessage);

            var httpClient = new HttpClient(handlerMock.Object);

            //_ = this.MockedHttpClient.Setup(x => x.PostAsync(It.IsAny<string>(), It.IsAny<HttpContent>())).ReturnsAsync(httpresponsemessage);
            var authenticationHeader = await this.TokenHelper.Object.GetAuthenticationHeaderValueForSelectedAccount(It.Is<AccountCredential>(a => a.AccountName.Equals("Test"))).ConfigureAwait(false);
            var msGrpahProvider = new MSGraphProvider(this.MsGraphSetting, Options.Create(retrySetting), this.Logger, httpClient);
            var result = await msGrpahProvider.ProcessEmailRequestBatch(this.TestTokenHeader, graphRequests).ConfigureAwait(false);
            Assert.AreEqual(2, result.Where(x => x.Status == System.Net.HttpStatusCode.TooManyRequests).Count());
            Assert.IsTrue(result.Any(x => x.Error.Contains("Quota Exceeded")));
        }

        /// <summary>
        /// Processes the email request batch no active accounts.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Test]
        public async Task ProcessEmailRequestBatch_ErrorGettingToken_ExceededRetries()
        {
            var applicationAccounts = new List<ApplicationAccounts>()
            {
                new ApplicationAccounts()
                {
                    ApplicationName = this.ApplicationName,
                    ValidAppIds = Guid.NewGuid().ToString(),
                    FromOverride = "TestFrom",
                    Accounts = new List<AccountCredential>()
                    {
                        new AccountCredential()
                        {
                            AccountName = "Test", IsEnabled = true, PrimaryPassword = "Test",
                        },
                        new AccountCredential()
                        {
                            AccountName = "Test2", IsEnabled = true, PrimaryPassword = "Test",
                        },
                         new AccountCredential()
                        {
                            AccountName = "Test3", IsEnabled = true, PrimaryPassword = "Test",
                        },
                    },
                },
            };

            var retrySetting = new RetrySetting
            {
                MaxRetries = 10,
                TransientRetryCount = 2,
            };

            Dictionary<string, string> testConfigValues = new Dictionary<string, string>()
            {
                { "ApplicationAccounts", JsonConvert.SerializeObject(applicationAccounts) },
                { "RetrySetting:MaxRetries", "10" },
                { "RetrySetting:TransientRetryCount", "2" },
            };

            this.Configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(testConfigValues)
                .Build();
            var notificationId1 = Guid.NewGuid().ToString();
            var notificationId2 = Guid.NewGuid().ToString();

            var graphRequests = new GraphBatchRequest
            {
                Requests = new List<GraphRequest>
                {
                    new GraphRequest
                    {
                        Id = notificationId1,
                        Body = "Test Body",
                    },
                    new GraphRequest
                    {
                        Id = notificationId1,
                        Body = "Test Body",
                    },
                },
            };
            string res = null;
            _ = this.TokenHelper.Setup(x => x.GetAccessTokenForSelectedAccount(It.Is<AccountCredential>(a => a.AccountName.Equals("Test")))).ReturnsAsync("token");
            _ = this.TokenHelper.Setup(x => x.GetAccessTokenForSelectedAccount(It.Is<AccountCredential>(a => a.AccountName.Equals("Test2")))).ReturnsAsync("token");
            _ = this.TokenHelper.Setup(x => x.GetAccessTokenForSelectedAccount(It.Is<AccountCredential>(a => a.AccountName.Equals("Test3")))).ReturnsAsync("token");
            _ = this.TokenHelper.Setup(x => x.GetAuthenticationHeaderFromToken(It.IsAny<string>())).ReturnsAsync(new System.Net.Http.Headers.AuthenticationHeaderValue("test"));


            var httpresponsemessage = new HttpResponseMessage
            {
                Content = new StringContent(JsonConvert.SerializeObject(new GraphBatchResponse
                {
                    Responses = new List<GraphResponse>
                    {
                        new GraphResponse
                        {
                                            Status = System.Net.HttpStatusCode.TooManyRequests,
                                            Body = new GraphResponseBody { Error = new GraphResponseError { Code = "ErrorSendAsDenied", Messsage = "Quota Exceeded" } },
                                            Id = notificationId1,
                        },
                        new GraphResponse
                        {
                                            Status = System.Net.HttpStatusCode.TooManyRequests,
                                            Body = new GraphResponseBody { Error = new GraphResponseError { Code = "ErrorSendAsDenied", Messsage = "Quota Exceeded" } },
                                            Id = notificationId2,
                        },
                    },
                })),
            };

            var handlerMock = new Mock<HttpMessageHandler>();
            _ = handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>())
               .ReturnsAsync(httpresponsemessage);

            var httpClient = new HttpClient(handlerMock.Object);

            //_ = this.MockedHttpClient.Setup(x => x.PostAsync(It.IsAny<string>(), It.IsAny<HttpContent>())).ReturnsAsync(httpresponsemessage);
            var authenticationHeader = await this.TokenHelper.Object.GetAuthenticationHeaderValueForSelectedAccount(It.Is<AccountCredential>(a => a.AccountName.Equals("Test"))).ConfigureAwait(false);
            var msGrpahProvider = new MSGraphProvider(this.MsGraphSetting, Options.Create(retrySetting), this.Logger, httpClient);
            var result = await msGrpahProvider.ProcessEmailRequestBatch(this.TestTokenHeader, graphRequests).ConfigureAwait(false);
            Assert.AreEqual(2, result.Where(x => x.Status == System.Net.HttpStatusCode.TooManyRequests).Count());
            Assert.IsTrue(result.Any(x => x.Error.Contains("Quota Exceeded")));
        }

        /// <summary>
        /// Processes the email request batch no active accounts.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Test]
        public async Task ProcessEmailRequestBatch_Success()
        {
            var applicationAccounts = new List<ApplicationAccounts>()
            {
                new ApplicationAccounts()
                {
                    ApplicationName = this.ApplicationName,
                    ValidAppIds = Guid.NewGuid().ToString(),
                    FromOverride = "TestFrom",
                    Accounts = new List<AccountCredential>()
                    {
                        new AccountCredential()
                        {
                            AccountName = "Test", IsEnabled = true, PrimaryPassword = "Test",
                        },
                        new AccountCredential()
                        {
                            AccountName = "Test2", IsEnabled = true, PrimaryPassword = "Test",
                        },
                         new AccountCredential()
                        {
                            AccountName = "Test3", IsEnabled = true, PrimaryPassword = "Test",
                        },
                    },
                },
            };

            var retrySetting = new RetrySetting
            {
                MaxRetries = 10,
                TransientRetryCount = 2,
            };

            Dictionary<string, string> testConfigValues = new Dictionary<string, string>()
            {
                { "ApplicationAccounts", JsonConvert.SerializeObject(applicationAccounts) },
                { "RetrySetting:MaxRetries", "10" },
                { "RetrySetting:TransientRetryCount", "2" },
            };

            this.Configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(testConfigValues)
                .Build();
            var notificationId1 = Guid.NewGuid().ToString();
            var notificationId2 = Guid.NewGuid().ToString();

            var graphRequests = new GraphBatchRequest
            {
                Requests = new List<GraphRequest>
                {
                    new GraphRequest
                    {
                        Id = notificationId1,
                        Body = "Test Body",
                    },
                    new GraphRequest
                    {
                        Id = notificationId1,
                        Body = "Test Body",
                    },
                },
            };
            string res = null;
            _ = this.TokenHelper.Setup(x => x.GetAccessTokenForSelectedAccount(It.Is<AccountCredential>(a => a.AccountName.Equals("Test")))).ReturnsAsync("token");
            _ = this.TokenHelper.Setup(x => x.GetAccessTokenForSelectedAccount(It.Is<AccountCredential>(a => a.AccountName.Equals("Test2")))).ReturnsAsync("token");
            _ = this.TokenHelper.Setup(x => x.GetAccessTokenForSelectedAccount(It.Is<AccountCredential>(a => a.AccountName.Equals("Test3")))).ReturnsAsync("token");
            _ = this.TokenHelper.Setup(x => x.GetAuthenticationHeaderFromToken(It.IsAny<string>())).ReturnsAsync(new System.Net.Http.Headers.AuthenticationHeaderValue("test"));


            var httpresponsemessage = new HttpResponseMessage
            {
                Content = new StringContent(JsonConvert.SerializeObject(new GraphBatchResponse
                {
                    Responses = new List<GraphResponse>
                    {
                        new GraphResponse
                        {
                                            Status = System.Net.HttpStatusCode.Accepted,
                                            Body = null,
                                            Id = notificationId1,
                        },
                        new GraphResponse
                        {
                                            Status = System.Net.HttpStatusCode.Accepted,
                                            Body = null,
                                            Id = notificationId2,
                        },
                    },
                })),
            };

            var handlerMock = new Mock<HttpMessageHandler>();
            _ = handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>())
               .ReturnsAsync(httpresponsemessage);

            var httpClient = new HttpClient(handlerMock.Object);

            var msGrpahProvider = new MSGraphProvider(this.MsGraphSetting, Options.Create(retrySetting), this.Logger, httpClient, this.TokenHelper.Object, this.Configuration);
            var result = await msGrpahProvider.ProcessEmailRequestBatch(this.ApplicationName, applicationAccounts, graphRequests).ConfigureAwait(false);
            Assert.AreEqual(2, result.Where(x => x.Status == System.Net.HttpStatusCode.Accepted).Count());
        }

        /// <summary>
        /// Processes the email request batch no active accounts.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Test]
        public async Task ProcessEmailRequestBatch_Respone_Null()
        {
            var applicationAccounts = new List<ApplicationAccounts>()
            {
                new ApplicationAccounts()
                {
                    ApplicationName = this.ApplicationName,
                    ValidAppIds = Guid.NewGuid().ToString(),
                    FromOverride = "TestFrom",
                    Accounts = new List<AccountCredential>()
                    {
                        new AccountCredential()
                        {
                            AccountName = "Test", IsEnabled = true, PrimaryPassword = "Test",
                        },
                        new AccountCredential()
                        {
                            AccountName = "Test2", IsEnabled = true, PrimaryPassword = "Test",
                        },
                        new AccountCredential()
                        {
                            AccountName = "Test3", IsEnabled = true, PrimaryPassword = "Test",
                        },
                    },
                },
            };

            var retrySetting = new RetrySetting
            {
                MaxRetries = 10,
                TransientRetryCount = 2,
            };

            Dictionary<string, string> testConfigValues = new Dictionary<string, string>()
            {
                { "ApplicationAccounts", JsonConvert.SerializeObject(applicationAccounts) },
                { "RetrySetting:MaxRetries", "10" },
                { "RetrySetting:TransientRetryCount", "2" },
            };

            this.Configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(testConfigValues)
                .Build();
            var notificationId1 = Guid.NewGuid().ToString();
            var notificationId2 = Guid.NewGuid().ToString();

            var graphRequests = new GraphBatchRequest
            {
                Requests = new List<GraphRequest>
                {
                    new GraphRequest
                    {
                        Id = notificationId1,
                        Body = "Test Body",
                    },
                    new GraphRequest
                    {
                        Id = notificationId1,
                        Body = "Test Body",
                    },
                },
            };
            string res = null;
            _ = this.TokenHelper.Setup(x => x.GetAccessTokenForSelectedAccount(It.Is<AccountCredential>(a => a.AccountName.Equals("Test")))).ReturnsAsync("token");
            _ = this.TokenHelper.Setup(x => x.GetAccessTokenForSelectedAccount(It.Is<AccountCredential>(a => a.AccountName.Equals("Test2")))).ReturnsAsync("token");
            _ = this.TokenHelper.Setup(x => x.GetAccessTokenForSelectedAccount(It.Is<AccountCredential>(a => a.AccountName.Equals("Test3")))).ReturnsAsync("token");
            _ = this.TokenHelper.Setup(x => x.GetAuthenticationHeaderFromToken(It.IsAny<string>())).ReturnsAsync(new System.Net.Http.Headers.AuthenticationHeaderValue("test"));


            //var httpresponsemessage = new HttpResponseMessage
            //{
            //    Content = new StringContent(JsonConvert.SerializeObject(new GraphBatchResponse
            //    {
            //        Responses = new List<GraphResponse>
            //        {
            //            new GraphResponse
            //            {
            //                                Status = System.Net.HttpStatusCode.TooManyRequests,
            //                                Body = new GraphResponseBody { Error = new GraphResponseError { Code = "ErrorSendAsDenied", Messsage = "Quota Exceeded" } },
            //                                Id = notificationId1,
            //            },
            //            new GraphResponse
            //            {
            //                                Status = System.Net.HttpStatusCode.TooManyRequests,
            //                                Body = new GraphResponseBody { Error = new GraphResponseError { Code = "ErrorSendAsDenied", Messsage = "Quota Exceeded" } },
            //                                Id = notificationId2,
            //            },
            //        },
            //    })),
            //};

            var handlerMock = new Mock<HttpMessageHandler>();
            _ = handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>())
               .ReturnsAsync(new HttpResponseMessage());

            var httpClient = new HttpClient(handlerMock.Object);

            //_ = this.MockedHttpClient.Setup(x => x.PostAsync(It.IsAny<string>(), It.IsAny<HttpContent>())).ReturnsAsync(httpresponsemessage);

            var msGrpahProvider = new MSGraphProvider(this.MsGraphSetting, Options.Create(retrySetting), this.Logger, httpClient, this.TokenHelper.Object, this.Configuration);
            var result = await msGrpahProvider.ProcessEmailRequestBatch(this.ApplicationName, applicationAccounts, graphRequests).ConfigureAwait(false);
            Assert.AreEqual(0, result.Count());

            var ms = new HttpResponseMessage(statusCode: System.Net.HttpStatusCode.BadRequest);
            ms.Content = new StringContent("BadRequest");

            _ = handlerMock
              .Protected()
              .Setup<Task<HttpResponseMessage>>(
                 "SendAsync",
                 ItExpr.IsAny<HttpRequestMessage>(),
                 ItExpr.IsAny<CancellationToken>())
              .ReturnsAsync(ms);

            httpClient = new HttpClient(handlerMock.Object);

            //_ = this.MockedHttpClient.Setup(x => x.PostAsync(It.IsAny<string>(), It.IsAny<HttpContent>())).ReturnsAsync(httpresponsemessage);

            msGrpahProvider = new MSGraphProvider(this.MsGraphSetting, Options.Create(retrySetting), this.Logger, httpClient, this.TokenHelper.Object, this.Configuration);
            var ex = Assert.ThrowsAsync<Exception>(() => msGrpahProvider.ProcessEmailRequestBatch(this.ApplicationName, applicationAccounts, graphRequests));
            Assert.IsTrue(ex.Message.Contains("BadRequest"));
        }
    }
}
*/