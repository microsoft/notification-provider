// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.UnitTests.Service.Provider
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Security.Claims;
    using Microsoft.AspNetCore.Connections;
    using Microsoft.AspNetCore.SignalR;
    using Microsoft.Extensions.Logging;
    using Moq;
    using NUnit.Framework;
    using WebNotifications.Providers;

    /// <summary>
    /// GetUserId test for UserObjectIdentifier.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class UserObjectIdentifierProvider_GetUserId_Tests
    {
        private Mock<ConnectionContext> contextMock;
        private Mock<ILoggerFactory> loggerFactoryMock;
        private Mock<HubConnectionContext> connectionContextMock;

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        [SetUp]
        public void Initialize()
        {
            this.loggerFactoryMock = new Mock<ILoggerFactory>();
            this.contextMock = new Mock<ConnectionContext>();
            this.connectionContextMock = new Mock<HubConnectionContext>(this.contextMock.Object, new HubConnectionContextOptions(), this.loggerFactoryMock.Object);
        }

        /// <summary>
        /// Gets the user identifier with a null connection.
        /// </summary>
        [Test]
        public void GetUserId_NullConnection()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => new UserObjectIdentifierProvider().GetUserId(null));
            Assert.IsTrue(ex.ParamName.Equals("connection"));
        }

        /// <summary>
        /// Gets the user identifier with oid.
        /// </summary>
        /// <param name="claimType">Type of the claim.</param>
        [TestCase("oid")]
        [TestCase("http://schemas.microsoft.com/identity/claims/objectidentifier")]
        public void GetUserId_WithOid(string claimType)
        {
            string objectId = Guid.NewGuid().ToString();
            Claim claim = new Claim(claimType, objectId);
            ClaimsIdentity claimsIdentity = new ClaimsIdentity(new List<Claim> { claim });
            ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            _ = this.connectionContextMock.SetupGet(x => x.User).Returns(claimsPrincipal);
            UserObjectIdentifierProvider userObjectIdentifierProvider = new UserObjectIdentifierProvider();
            string userId = userObjectIdentifierProvider.GetUserId(this.connectionContextMock.Object);
            Assert.IsTrue(userId.Equals(objectId, StringComparison.Ordinal));
        }
    }
}
