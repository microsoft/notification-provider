// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace DirectSend.Tests
{
    using MailKit;
    using MailKit.Net.Smtp;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using MimeKit;
    using Moq;
    using DirectSend.Models.Configurations;
    using DirectSend.Models.Mail;
    using NotificationProviders.Common.Logger;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    [TestClass()]
    public class DirectSendMailServiceTests
    {
        readonly Mock<ISmtpClientPool> mockedClientPool = new Mock<ISmtpClientPool>();
        readonly Mock<ILogger> mockedLogger = new Mock<ILogger>();
        readonly Mock<IDSSmtpClient> mockedClient = new Mock<IDSSmtpClient>();
        [TestMethod()]
        public async Task SendAsyncTest()
        {
            SendAccountConfiguration sendAccountConfiguration = new SendAccountConfiguration { Address = "TestAddress", FromAddressDisplayName = "TestDisplayName" };
            var directSendMailService= new DirectSendMailService(this.mockedClientPool.Object, this.mockedLogger.Object, sendAccountConfiguration);
            var emailMessage = new EmailMessage
            {
                Content = "TestContent",
                FromAddresses = new List<EmailAddress> { new EmailAddress { Address = "fromAddress@microsoft.com", Name = "FromName" } },
                ToAddresses = new List<EmailAddress> { new EmailAddress { Address = "toAddress@microsoft.com", Name = "ToName" } },
                Subject = "emailSubject",
            };
            var smtpConfig = new SmtpConfiguration { SmtpServer = "Test", SmtpPort = 25 };

            _ = this.mockedClientPool.Setup(x => x.GetClient(It.IsAny<Dictionary<string, string>>())).ReturnsAsync(this.mockedClient.Object);
            await directSendMailService.SendEmailAsync(emailMessage);

            this.mockedClient.Verify(x => x.SendAsync(It.Is<MimeMessage>(q => q.To.Mailboxes.Any(t => t.Address.Equals("toAddress@microsoft.com"))), It.IsAny<Dictionary<string, string>>(), null, default), Times.Once);
            this.mockedClient.Verify(x => x.SendAsync(It.Is<MimeMessage>(q => q.From.Mailboxes.Any(t => t.Address.Equals("fromAddress@microsoft.com"))), It.IsAny<Dictionary<string, string>>(), null, default), Times.Once);
            this.mockedClient.Verify(x => x.SendAsync(It.Is<MimeMessage>(q => q.Subject.Equals("emailSubject")), It.IsAny<Dictionary<string, string>>(), null, default), Times.Once);
        }

        [TestMethod()]
        public async Task SendMeetingAsyncTest()
        {
            SendAccountConfiguration sendAccountConfiguration = new SendAccountConfiguration { Address = "TestAddress", FromAddressDisplayName = "TestDisplayName" };
            var directSendMailService = new DirectSendMailService(this.mockedClientPool.Object, this.mockedLogger.Object, sendAccountConfiguration);
            var emailMessage = new EmailMessage
            {
                Content = "TestContent",
                FromAddresses = new List<EmailAddress> { new EmailAddress { Address = "fromAddress@microsoft.com", Name = "FromName" } },
                ToAddresses = new List<EmailAddress> { new EmailAddress { Address = "toAddress@microsoft.com", Name = "ToName" } },
                Subject = "emailSubject",
            };
            var smtpConfig = new SmtpConfiguration { SmtpServer = "Test", SmtpPort = 25 };

            _ = this.mockedClientPool.Setup(x => x.GetClient(It.IsAny<Dictionary<string, string>>())).ReturnsAsync(this.mockedClient.Object);
            await directSendMailService.SendMeetingInviteAsync(emailMessage);

            this.mockedClient.Verify(x => x.SendAsync(It.Is<MimeMessage>(q => q.To.Mailboxes.Any(t => t.Address.Equals("toAddress@microsoft.com"))), It.IsAny<Dictionary<string, string>>(), null, default), Times.Once);
            this.mockedClient.Verify(x => x.SendAsync(It.Is<MimeMessage>(q => q.From.Mailboxes.Any(t => t.Address.Equals("fromAddress@microsoft.com"))), It.IsAny<Dictionary<string, string>>(), null, default), Times.Once);
            this.mockedClient.Verify(x => x.SendAsync(It.Is<MimeMessage>(q => q.Subject.Equals("emailSubject")), It.IsAny<Dictionary<string, string>>(), null, default), Times.Once);
        }

        [TestMethod()]
        public async Task SendAsyncTest_Attachments()
        {
            SendAccountConfiguration sendAccountConfiguration = new SendAccountConfiguration { Address = "TestAddress", FromAddressDisplayName = "TestDisplayName" };
            var directSendMailService = new DirectSendMailService(this.mockedClientPool.Object, this.mockedLogger.Object, sendAccountConfiguration);
            var emailMessage = new EmailMessage
            {
                Content = "TestContent",
                FromAddresses = new List<EmailAddress> { new EmailAddress { Address = "fromAddress@microsoft.com", Name = "FromName" } },
                ToAddresses = new List<EmailAddress> { new EmailAddress { Address = "toAddress@microsoft.com", Name = "ToName" } },
                Subject = "emailSubject",
                FileName = new List<string> { "TestDocument.docx" },
                FileContent = new List<string> { Convert.ToBase64String(File.ReadAllBytes(Path.GetFullPath(Directory.GetCurrentDirectory() + @"\TestDocument.docx"))) },
            };
            var smtpConfig = new SmtpConfiguration { SmtpServer = "Test", SmtpPort = 25 };

            _ = this.mockedClientPool.Setup(x => x.GetClient(It.IsAny<Dictionary<string, string>>())).ReturnsAsync(this.mockedClient.Object);
            await directSendMailService.SendEmailAsync(emailMessage);

            this.mockedClient.Verify(x => x.SendAsync(It.Is<MimeMessage>(q => q.To.Mailboxes.Any(t => t.Address.Equals("toAddress@microsoft.com"))), It.IsAny<Dictionary<string, string>>(), null, default), Times.Once);
            this.mockedClient.Verify(x => x.SendAsync(It.Is<MimeMessage>(q => q.From.Mailboxes.Any(t => t.Address.Equals("fromAddress@microsoft.com"))), It.IsAny<Dictionary<string, string>>(), null, default), Times.Once);
            this.mockedClient.Verify(x => x.SendAsync(It.Is<MimeMessage>(q => q.Subject.Equals("emailSubject")), It.IsAny<Dictionary<string, string>>(), null, default), Times.Once);
        }

        [TestMethod()]
        public async Task SendAsyncTest_Exception()
        {
            SendAccountConfiguration sendAccountConfiguration = new SendAccountConfiguration { Address = "TestAddress", FromAddressDisplayName = "TestDisplayName" };
            var directSendMailService = new DirectSendMailService(this.mockedClientPool.Object, this.mockedLogger.Object, sendAccountConfiguration);
            var emailMessage = new EmailMessage
            {
                Content = "TestContent",
                FromAddresses = new List<EmailAddress> { new EmailAddress { Address = "fromAddress@microsoft.com", Name = "FromName" } },
                ToAddresses = new List<EmailAddress> { new EmailAddress { Address = "toAddress@microsoft.com", Name = "ToName" } },
                Subject = "emailSubject",
            };
            var smtpConfig = new SmtpConfiguration { SmtpServer = "Test", SmtpPort = 25 };
            _ = this.mockedClient.Setup(x => x.SendAsync(It.IsAny<MimeMessage>(), It.IsAny<Dictionary<string, string>>(), null, default)).ThrowsAsync(new Exception("test exception"));
            _ = this.mockedClientPool.Setup(x => x.GetClient(It.IsAny<Dictionary<string, string>>())).ReturnsAsync(this.mockedClient.Object);
            var ex = await Assert.ThrowsExceptionAsync<Exception>(() =>  directSendMailService.SendEmailAsync(emailMessage));

            Assert.AreEqual("test exception", ex.Message);
            _ = this.mockedClient.Setup(x => x.SendAsync(It.IsAny<MimeMessage>(), It.IsAny<Dictionary<string, string>>(), null, default)).ThrowsAsync(new SmtpCommandException(SmtpErrorCode.MessageNotAccepted, SmtpStatusCode.AuthenticationChallenge, "Test"));
            _ = this.mockedClientPool.Setup(x => x.GetClient(It.IsAny<Dictionary<string, string>>())).ReturnsAsync(this.mockedClient.Object);
            var ex1 = await Assert.ThrowsExceptionAsync<SmtpCommandException>(() => directSendMailService.SendEmailAsync(emailMessage));

            Assert.AreEqual("Test", ex1.Message);
            Assert.IsTrue(ex1.ErrorCode == SmtpErrorCode.MessageNotAccepted);

            _ = this.mockedClient.Setup(x => x.SendAsync(It.IsAny<MimeMessage>(), It.IsAny<Dictionary<string, string>>(), null, default)).ThrowsAsync(new SmtpCommandException(SmtpErrorCode.RecipientNotAccepted, SmtpStatusCode.AuthenticationChallenge, "Test"));
            _ = this.mockedClientPool.Setup(x => x.GetClient(It.IsAny<Dictionary<string, string>>())).ReturnsAsync(this.mockedClient.Object);
            ex1 = await Assert.ThrowsExceptionAsync<SmtpCommandException>(() => directSendMailService.SendEmailAsync(emailMessage));

            Assert.AreEqual("Test", ex1.Message);
            Assert.IsTrue(ex1.ErrorCode == SmtpErrorCode.RecipientNotAccepted);

            _ = this.mockedClient.Setup(x => x.SendAsync(It.IsAny<MimeMessage>(), It.IsAny<Dictionary<string, string>>(), null, default)).ThrowsAsync(new SmtpCommandException(SmtpErrorCode.SenderNotAccepted, SmtpStatusCode.AuthenticationChallenge, "Test"));
            _ = this.mockedClientPool.Setup(x => x.GetClient(It.IsAny<Dictionary<string, string>>())).ReturnsAsync(this.mockedClient.Object);
            ex1 = await Assert.ThrowsExceptionAsync<SmtpCommandException>(() => directSendMailService.SendEmailAsync(emailMessage));

            Assert.AreEqual("Test", ex1.Message);
            Assert.IsTrue(ex1.ErrorCode == SmtpErrorCode.SenderNotAccepted);

            _ = this.mockedClient.Setup(x => x.SendAsync(It.IsAny<MimeMessage>(), It.IsAny<Dictionary<string, string>>(), null, default)).ThrowsAsync(new SmtpProtocolException("Test M"));
            _ = this.mockedClientPool.Setup(x => x.GetClient(It.IsAny<Dictionary<string, string>>())).ReturnsAsync(this.mockedClient.Object);
            var ex2 = await Assert.ThrowsExceptionAsync<SmtpProtocolException>(() => directSendMailService.SendEmailAsync(emailMessage));

            Assert.AreEqual("Test M", ex2.Message);

            _ = this.mockedClient.Setup(x => x.SendAsync(It.IsAny<MimeMessage>(), It.IsAny<Dictionary<string, string>>(), null, default)).ThrowsAsync(new IOException("Test IO"));
            _ = this.mockedClientPool.Setup(x => x.GetClient(It.IsAny<Dictionary<string, string>>())).ReturnsAsync(this.mockedClient.Object);
            var ex3 = await Assert.ThrowsExceptionAsync<IOException>(() => directSendMailService.SendEmailAsync(emailMessage));

            Assert.AreEqual("Test IO", ex3.Message);

            _ = this.mockedClient.Setup(x => x.SendAsync(It.IsAny<MimeMessage>(), It.IsAny<Dictionary<string, string>>(), null, default)).ThrowsAsync(new ServiceNotConnectedException("Test IO"));
            _ = this.mockedClientPool.Setup(x => x.GetClient(It.IsAny<Dictionary<string, string>>())).ReturnsAsync(this.mockedClient.Object);
            var ex4 = await Assert.ThrowsExceptionAsync<ServiceNotConnectedException>(() => directSendMailService.SendEmailAsync(emailMessage));

            Assert.AreEqual("Test IO", ex4.Message);

        }
    }
}