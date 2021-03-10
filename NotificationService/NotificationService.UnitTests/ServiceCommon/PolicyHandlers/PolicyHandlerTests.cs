// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.UnitTests.SvCommon.PolicyHandlers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Moq;
    using Newtonsoft.Json;
    using NotificationService.Contracts;
    using NotificationService.SvCommon;
    using NUnit.Framework;

    /// <summary>
    /// Tests for the Policy Handlers.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class PolicyHandlerTests
    {
        private AppNameAuthorizePolicyHandler appNameAuthorizePolicyHandler;
        private AppIdAuthorizePolicyHandler appIdAuthorizePolicyHandler;
        private Mock<IHttpContextAccessor> contextAccessor;

        /// <summary>
        /// Gets or sets Configuration.
        /// </summary>
        private IConfiguration Configuration { get; set; }

        /// <summary>
        /// Initialization for the tests.
        /// </summary>
        [SetUp]
        public void Setup()
        {
            this.contextAccessor = new Mock<IHttpContextAccessor>();
            var context = new DefaultHttpContext();
            var applicationName = "TestApp";
            context.Request.RouteValues["applicationName"] = applicationName;
            _ = this.contextAccessor.Setup(_ => _.HttpContext).Returns(context);
            var applicationAccounts = new List<ApplicationAccounts>()
            {
                new ApplicationAccounts()
                {
                    ApplicationName = applicationName,
                    ValidAppIds = Guid.NewGuid().ToString(),
                    Accounts = new List<AccountCredential>()
                    {
                        new AccountCredential()
                        {
                            AccountName = "Test", IsEnabled = true, PrimaryPassword = "Test",
                        },
                    },
                },
            };
            Dictionary<string, string> testConfigValues = new Dictionary<string, string>()
            {
                { "ApplicationAccounts", JsonConvert.SerializeObject(applicationAccounts) },
            };
            this.Configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(testConfigValues)
                .Build();
            this.appNameAuthorizePolicyHandler = new AppNameAuthorizePolicyHandler(this.contextAccessor.Object, this.Configuration);
            this.appIdAuthorizePolicyHandler = new AppIdAuthorizePolicyHandler(this.contextAccessor.Object, this.Configuration);
        }

        /// <summary>
        /// Tests for the AppNameAuthorizeRequirement.
        /// </summary>
        [Test]
        public void AppNameAuthorizeRequirementTests()
        {
            var requirements = new[] { new AppNameAuthorizeRequirement() };
            var appUser = "user";
            var user = new ClaimsPrincipal(
                new ClaimsIdentity(
                    new Claim[]
                    {
                        new Claim(ClaimsIdentity.DefaultNameClaimType, appUser),
                    },
                    "Basic"));
            var authzContext = new AuthorizationHandlerContext(requirements, user, new object());
            Task result = this.appNameAuthorizePolicyHandler.HandleAsync(authzContext);
            Assert.AreEqual(result.Status.ToString(), "RanToCompletion");
            Assert.IsTrue(authzContext.HasSucceeded);
        }

        /// <summary>
        /// Tests for the AppIdAuthorizeRequirement.
        /// </summary>
        [Test]
        public void AppIdAuthorizeRequirementTests()
        {
            var requirements = new[] { new AppIdAuthorizeRequirement() };
            var appUser = "user";
            var user = new ClaimsPrincipal(
                new ClaimsIdentity(
                    new Claim[]
                    {
                        new Claim(ClaimsIdentity.DefaultNameClaimType, appUser),
                    },
                    "Basic"));
            var authzContext = new AuthorizationHandlerContext(requirements, user, new object());
            Task result = this.appIdAuthorizePolicyHandler.HandleAsync(authzContext);
            Assert.AreEqual(result.Status.ToString(), "RanToCompletion");
            Assert.IsFalse(authzContext.HasSucceeded);
        }
    }
}
