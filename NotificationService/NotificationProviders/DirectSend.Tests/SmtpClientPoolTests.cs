﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using DirectSend;
using DirectSend.Models.Configurations;
using NotificationProviders.Common.Logger;
using Org.BouncyCastle.Math.EC.Rfc7748;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DirectSend.Tests
{
    [TestClass()]
    public class SmtpClientPoolTests
    {
        private Mock<ILogger> mockedLogger = new Mock<ILogger>();
        private Mock<ISmtpClientFactory> mockedfactory = new Mock<ISmtpClientFactory>();
        [TestMethod()]
        public async Task GetClientTest()
        {
            var smtpConfig = new SmtpConfiguration { SmtpServer = "server", SmtpPort = 25 };
            this.mockedfactory.Setup(x => x.CreateClient(It.Is<ISmtpConfiguration>(t => t.SmtpPort == 25 && t.SmtpServer.Equals("server")), It.IsAny<ILogger>())).Returns(new Mock<IDSSmtpClient>().Object);
            var clientPool = new SmtpClientPool(smtpConfig, this.mockedLogger.Object, this.mockedfactory.Object);
            var dic = new Dictionary<string, string>();
            var client = await clientPool.GetClient(dic);
            Assert.IsNotNull(client);
        }

        [TestMethod()]
        public async Task GetClientTest_2()
        {
            var smtpConfig = new SmtpConfiguration { SmtpServer = "server", SmtpPort = 25 };
            //mockedfactory.Setup(x => x.CreateClient(It.Is<ISmtpConfiguration>(t => t.SmtpPort == 25 && t.SmtpServer.Equals("server")), It.IsAny<ILogger>())).Returns(new Mock<IDSSmtpClient>().Object);
            var clientPool = new SmtpClientPool(smtpConfig, this.mockedLogger.Object, this.mockedfactory.Object);
            var dic = new Dictionary<string, string>();
            var client = await clientPool.GetClient(dic);
            Assert.IsNull(client);
        }

        [TestMethod()]
        public async Task ReturnClientTest()
        {
            var smtpConfig = new SmtpConfiguration { SmtpServer = "server", SmtpPort = 25 };
            //mockedfactory.Setup(x => x.CreateClient(It.Is<ISmtpConfiguration>(t => t.SmtpPort == 25 && t.SmtpServer.Equals("server")), It.IsAny<ILogger>())).Returns(new Mock<IDSSmtpClient>().Object);
            var clientPool = new SmtpClientPool(smtpConfig, this.mockedLogger.Object, this.mockedfactory.Object);
            var client = new Mock<IDSSmtpClient>().Object;
            var dic = new Dictionary<string, string>();
            await clientPool.ReturnClient(client, new Dictionary<string, string>());
        }
    }
}