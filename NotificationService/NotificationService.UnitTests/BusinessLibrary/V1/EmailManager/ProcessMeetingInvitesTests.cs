// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.UnitTests.BusinessLibrary.V1.EmailManager
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using Moq;
    using Newtonsoft.Json;
    using NotificationService.BusinessLibrary.Models;
    using NotificationService.BusinessLibrary.Providers;
    using NotificationService.Common;
    using NotificationService.Common.Configurations;
    using NotificationService.Contracts;
    using NotificationService.Contracts.Entities;
    using NotificationService.Contracts.Models.Graph.Invite;
    using NUnit.Framework;

    /// <summary>
    /// Unit Test class for Processing meeting Invites using MSGraphNotificationProvider.
    /// </summary>
    public class ProcessMeetingInvitesTests : EmailManagerTestsBase
    {
        /// <summary>
        /// RetrySetting Configuration.
        /// </summary>
        private readonly RetrySetting retrySetting = new RetrySetting
        {
            MaxRetries = 10,
            TransientRetryCount = 3,
        };

        /// <summary>
        /// Initialization for the tests.
        /// </summary>
        [SetUp]
        public void Setup() => this.SetupTestBase();

        /// <summary>
        /// Tests for ProcessMeetingNotifications with entities as null/empty.
        /// </summary>
        /// <returns> A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Test]
        public async Task TestProcessMeetingInvites_InviteItems_NullOrEmpty()
        {
            Exception ex = null;
            var testClass = new MSGraphNotificationProvider(this.Configuration, this.EmailAccountManager.Object, this.Logger, this.MsGraphSetting, this.TokenHelper.Object, this.MsGraphProvider.Object, this.EmailManager);
            try
            {
                await testClass.ProcessMeetingNotificationEntities(this.ApplicationName, null);
            }
            catch (ArgumentNullException ane)
            {
                ex = ane;
            }

            Assert.IsTrue(ex?.Message?.Contains("meetingInviteEntities are null/empty."));
        }

        /// <summary>
        /// Tests for ProcessMeetingNotifications method with AuthHeader as null.
        /// </summary>
        /// <returns> A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Test]
        public async Task TestProcessMeetingInvites_AuthHeader_Null()
        {
            var applicationAccounts = this.GetApplicationAccounts("abc@test.com", "testacc@test.com", "Test");
            Dictionary<string, string> testConfigValues = new Dictionary<string, string>()
            {
                { "ApplicationAccounts", JsonConvert.SerializeObject(applicationAccounts) },
                { "RetrySetting", JsonConvert.SerializeObject(this.retrySetting) },
            };
            var inviteEntities = this.GetMeetingNotificationItems(NotificationItemStatus.Sent);
            var testClass = new MSGraphNotificationProvider(this.Configuration, this.EmailAccountManager.Object, this.Logger, this.MsGraphSetting, this.TokenHelper.Object, this.MsGraphProvider.Object, this.EmailManager);
            await testClass.ProcessMeetingNotificationEntities(this.ApplicationName, inviteEntities);
            Assert.IsTrue(inviteEntities.FirstOrDefault().Status == NotificationItemStatus.Failed);
        }

        /// <summary>
        /// Tests ProcessMeetingNotifications without attachment resulting success.
        /// </summary>
        /// <returns> A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Test]
        public async Task TestProcessMeetingInvites_StatusSuccess_WithoutAttachment()
        {
            var applicationAccounts = this.GetApplicationAccounts("abc@test.com", "testacc@test.com", "Test");
            Dictionary<string, string> testConfigValues = new Dictionary<string, string>()
            {
                { "ApplicationAccounts", JsonConvert.SerializeObject(applicationAccounts) },
                { "RetrySetting", JsonConvert.SerializeObject(this.retrySetting) },
            };
            var res = new ResponseData<string>()
            {
                Status = true,
                StatusCode = HttpStatusCode.OK,
            };

            _ = this.TokenHelper.Setup(x => x.GetAuthenticationHeaderValueForSelectedAccount(It.IsAny<AccountCredential>())).ReturnsAsync(new AuthenticationHeaderValue("test"));
            _ = this.MsGraphProvider.Setup(x => x.SendMeetingInvite(It.IsAny<AuthenticationHeaderValue>(), It.IsAny<InvitePayload>(), It.IsAny<string>())).ReturnsAsync(res);
            var inviteEntities = this.GetMeetingNotificationItems(NotificationItemStatus.Queued);
            var testClass = new MSGraphNotificationProvider(this.Configuration, this.EmailAccountManager.Object, this.Logger, this.MsGraphSetting, this.TokenHelper.Object, this.MsGraphProvider.Object, this.EmailManager);
            await testClass.ProcessMeetingNotificationEntities(this.ApplicationName, inviteEntities);
            Assert.IsTrue(inviteEntities.FirstOrDefault().Status == NotificationItemStatus.Sent);
        }

        /// <summary>
        /// Tests ProcessMeetingNotifications without attachment resulting Failure.
        /// </summary>
        /// <returns> A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Test]
        public async Task TestProcessMeetingInvites_StatusFailed_WithoutAttachment()
        {
            var applicationAccounts = this.GetApplicationAccounts("abc@test.com", "testacc@test.com", "Test");
            Dictionary<string, string> testConfigValues = new Dictionary<string, string>()
            {
                { "ApplicationAccounts", JsonConvert.SerializeObject(applicationAccounts) },
                { "RetrySetting", JsonConvert.SerializeObject(this.retrySetting) },
            };
            var res = new ResponseData<string>()
            {
                Status = false,
                StatusCode = HttpStatusCode.TooManyRequests,
            };
            _ = this.TokenHelper.Setup(x => x.GetAuthenticationHeaderValueForSelectedAccount(It.IsAny<AccountCredential>())).ReturnsAsync(new AuthenticationHeaderValue("test"));
            _ = this.MsGraphProvider.Setup(x => x.SendMeetingInvite(It.IsAny<AuthenticationHeaderValue>(), It.IsAny<InvitePayload>(), It.IsAny<string>())).ReturnsAsync(res);
            var inviteEntities = this.GetMeetingNotificationItems(NotificationItemStatus.Queued);
            var testClass = new MSGraphNotificationProvider(this.Configuration, this.EmailAccountManager.Object, this.Logger, this.MsGraphSetting, this.TokenHelper.Object, this.MsGraphProvider.Object, this.EmailManager);
            await testClass.ProcessMeetingNotificationEntities(this.ApplicationName, inviteEntities);
            Assert.IsTrue(inviteEntities.FirstOrDefault().Status == NotificationItemStatus.Retrying);
        }

        /// <summary>
        /// Tests ProcessMeetingNotifications with attachment resulting Failure.
        /// </summary>
        /// <returns> A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Test]
        public async Task TestProcessMeetingInvites_StatusSuccess_WithAttachment()
        {
            var applicationAccounts = this.GetApplicationAccounts("abc@test.com", "testacc@test.com", "Test");
            Dictionary<string, string> testConfigValues = new Dictionary<string, string>()
            {
                { "ApplicationAccounts", JsonConvert.SerializeObject(applicationAccounts) },
                { "RetrySetting", JsonConvert.SerializeObject(this.retrySetting) },
            };
            var res = new ResponseData<string>()
            {
                Status = true,
                StatusCode = HttpStatusCode.OK,
            };

            _ = this.TokenHelper.Setup(x => x.GetAuthenticationHeaderValueForSelectedAccount(It.IsAny<AccountCredential>())).ReturnsAsync(new AuthenticationHeaderValue("test"));
            _ = this.MsGraphProvider.Setup(x => x.SendMeetingInvite(It.IsAny<AuthenticationHeaderValue>(), It.IsAny<InvitePayload>(), It.IsAny<string>())).ReturnsAsync(res);
            var inviteEntities = this.GetMeetingNotificationItems(NotificationItemStatus.Queued);
            inviteEntities.FirstOrDefault().Attachments = this.GetAttachments();
            var attachmentRes = new Dictionary<string, ResponseData<string>>()
            {
                { inviteEntities.FirstOrDefault().Attachments.FirstOrDefault().FileName, new ResponseData<string>() { Status = true, StatusCode = HttpStatusCode.OK } },
            };
            _ = this.MsGraphProvider.Setup(x => x.SendMeetingInviteAttachments(It.IsAny<AuthenticationHeaderValue>(), It.IsAny<List<FileAttachment>>(), It.IsAny<string>(), It.IsAny<string>())).Returns(attachmentRes);

            var testClass = new MSGraphNotificationProvider(this.Configuration, this.EmailAccountManager.Object, this.Logger, this.MsGraphSetting, this.TokenHelper.Object, this.MsGraphProvider.Object, this.EmailManager);
            await testClass.ProcessMeetingNotificationEntities(this.ApplicationName, inviteEntities);
            Assert.IsTrue(inviteEntities.FirstOrDefault().Status == NotificationItemStatus.Sent);
        }

        /// <summary>
        /// Tests ProcessMeetingNotifications with attachment resulting Failure with Status as Failed ( not further retry).
        /// </summary>
        /// <returns> A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Test]
        public async Task TestProcessMeetingInvites_StatusFailed_NoFurtherRetry()
        {
            var applicationAccounts = this.GetApplicationAccounts("abc@test.com", "testacc@test.com", "Test");
            Dictionary<string, string> testConfigValues = new Dictionary<string, string>()
            {
                { "ApplicationAccounts", JsonConvert.SerializeObject(applicationAccounts) },
                { "RetrySetting", JsonConvert.SerializeObject(this.retrySetting) },
            };
            var res = new ResponseData<string>()
            {
                Status = false,
                StatusCode = HttpStatusCode.OK,
            };

            _ = this.TokenHelper.Setup(x => x.GetAuthenticationHeaderValueForSelectedAccount(It.IsAny<AccountCredential>())).ReturnsAsync(new AuthenticationHeaderValue("test"));
            _ = this.MsGraphProvider.Setup(x => x.SendMeetingInvite(It.IsAny<AuthenticationHeaderValue>(), It.IsAny<InvitePayload>(), It.IsAny<string>())).ReturnsAsync(res);
            var inviteEntities = this.GetMeetingNotificationItems(NotificationItemStatus.Queued);
            inviteEntities.FirstOrDefault().TryCount = 10;
            var testClass = new MSGraphNotificationProvider(this.Configuration, this.EmailAccountManager.Object, this.Logger, this.MsGraphSetting, this.TokenHelper.Object, this.MsGraphProvider.Object, this.EmailManager);
            await testClass.ProcessMeetingNotificationEntities(this.ApplicationName, inviteEntities);
            Assert.IsTrue(inviteEntities.FirstOrDefault().Status == NotificationItemStatus.Failed);
        }

        /// <summary>
        /// Tests ProcessMeetingNotifications with attachment resulting Failure.
        /// </summary>
        /// <returns> A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Test]
        public async Task TestProcessMeetingInvites_StatusFailed_WithAttachment()
        {
            var applicationAccounts = this.GetApplicationAccounts("abc@test.com", "testacc@test.com", "Test");
            Dictionary<string, string> testConfigValues = new Dictionary<string, string>()
            {
                { "ApplicationAccounts", JsonConvert.SerializeObject(applicationAccounts) },
                { "RetrySetting", JsonConvert.SerializeObject(this.retrySetting) },
            };
            var res = new ResponseData<string>()
            {
                Status = true,
                StatusCode = HttpStatusCode.OK,
            };

            _ = this.TokenHelper.Setup(x => x.GetAuthenticationHeaderValueForSelectedAccount(It.IsAny<AccountCredential>())).ReturnsAsync(new AuthenticationHeaderValue("test"));
            _ = this.MsGraphProvider.Setup(x => x.SendMeetingInvite(It.IsAny<AuthenticationHeaderValue>(), It.IsAny<InvitePayload>(), It.IsAny<string>())).ReturnsAsync(res);
            var inviteEntities = this.GetMeetingNotificationItems(NotificationItemStatus.Queued);
            inviteEntities.FirstOrDefault().Attachments = this.GetAttachments();
            var attachmentRes = new Dictionary<string, ResponseData<string>>()
            {
                { inviteEntities.FirstOrDefault().Attachments.FirstOrDefault().FileName, new ResponseData<string>() { Status = false, StatusCode = HttpStatusCode.BadRequest } },
            };
            _ = this.MsGraphProvider.Setup(x => x.SendMeetingInviteAttachments(It.IsAny<AuthenticationHeaderValue>(), It.IsAny<List<FileAttachment>>(), It.IsAny<string>(), It.IsAny<string>())).Returns(attachmentRes);
            _ = this.MsGraphProvider.Setup(x => x.DeleteMeetingInvite(It.IsAny<AuthenticationHeaderValue>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new ResponseData<string>() { Status = false, StatusCode = HttpStatusCode.OK });
            var testClass = new MSGraphNotificationProvider(this.Configuration, this.EmailAccountManager.Object, this.Logger, this.MsGraphSetting, this.TokenHelper.Object, this.MsGraphProvider.Object, this.EmailManager);
            await testClass.ProcessMeetingNotificationEntities(this.ApplicationName, inviteEntities);
            Assert.IsTrue(inviteEntities.FirstOrDefault().Status == NotificationItemStatus.Retrying);
        }

        private List<ApplicationAccounts> GetApplicationAccounts(string fromOverride, string accountName, string primaryPassword, bool isEnabled = true)
        {
            return new List<ApplicationAccounts>()
            {
                new ApplicationAccounts()
                {
                    ApplicationName = this.ApplicationName,
                    ValidAppIds = Guid.NewGuid().ToString(),
                    FromOverride = fromOverride,
                    Accounts = new List<AccountCredential>()
                    {
                        new AccountCredential()
                        {
                            AccountName = accountName, IsEnabled = isEnabled, PrimaryPassword = primaryPassword,
                        },
                    },
                },
            };
        }

        private IList<MeetingNotificationItemEntity> GetMeetingNotificationItems(NotificationItemStatus status)
        {
            var notificationId2 = Guid.NewGuid().ToString();
            var date = DateTime.UtcNow;
            return new List<MeetingNotificationItemEntity>()
            {
                new MeetingNotificationItemEntity()
                {
                    Application = this.ApplicationName,
                    NotificationId = notificationId2,
                    Subject = "TestEmailSubject",
                    Body = "CfDJ8KvR5DP4DK5GqV1jviPzBnsv3onVDZ-ztz-AvRl_6nvVNg86jfmKjgySREDPW9xNrwpKALT5BIFNX6VK3wzKsxc51dbkQjPPG9l7436wQktrAMRadumTpGKNKG1lLlP0FA",
                    Id = notificationId2,
                    Status = status,
                    RequiredAttendees = "abc@test.com",
                    Start = date,
                    End = date.AddHours(1),
                    Priority = NotificationPriority.Normal,
                    Location = "Test",
                },
            };
        }

        private List<NotificationAttachmentEntity> GetAttachments()
        {
            return new List<NotificationAttachmentEntity>()
            {
                new NotificationAttachmentEntity()
                {
                    FileBase64 = "VEhpcyBpcyBhIHRlc3QgYXR0YWNobWVudCBmaWxlLg==",
                    FileName = "Test.txt",
                    IsInline = false,
                },
            };
        }

        private List<MailSettings> GetMailSettings()
        {
            return new List<MailSettings>()
            {
                new MailSettings()
                {
                    ApplicationName = "TestApp",
                    MailOn = true,
                    SendForReal = false,
                    ToOverride = "user@contoso.com",
                    SaveToSent = true,
                },
            };
        }
    }
}
