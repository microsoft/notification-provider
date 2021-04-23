// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.UnitTests.Common.Encryption
{
    using System;
    using System.Security.Cryptography;
    using System.Text;
    using Azure.Core.Cryptography;
    using Microsoft.Extensions.Configuration;
    using Moq;
    using NotificationService.Common.Configurations;
    using NotificationService.Common.Encryption;
    using NotificationService.UnitTests.Mocks;
    using NUnit.Framework;

    /// <summary>
    /// Tests for Encrypt/Decrypt methods of Common Encryption Service.
    /// </summary>
    public class EncryptionServiceTests
    {
        /// <summary>
        /// The mocked cryptography client.
        /// </summary>
        private Mock<IKeyEncryptionKey> mockedCryptographyClient;

        /// <summary>
        /// The mocked configuration.
        /// </summary>
        private Mock<IConfiguration> mockedConfiguration;

        /// <summary>
        /// Initialization for the tests.
        /// </summary>
        [SetUp]
        public void Setup()
        {
            this.mockedCryptographyClient = new Mock<IKeyEncryptionKey>();
            this.mockedConfiguration = new Mock<IConfiguration>();
            Aes myAes = Aes.Create();
            _ = this.mockedConfiguration.Setup(x => x[ConfigConstants.NotificationEncryptionKey]).Returns(Convert.ToBase64String(Encoding.UTF8.GetBytes("EncryptedKey")));
            _ = this.mockedConfiguration.Setup(x => x[ConfigConstants.NotificationEncryptionIntialVector]).Returns(Convert.ToBase64String(Encoding.UTF8.GetBytes("EncryptedIV")));
        }

        /// <summary>
        /// Tests for Encrypt method.
        /// </summary>
        [Test]
        public void EncryptDecryptTestValidInput()
        {
            EncryptionService encryptionService = new EncryptionService(new MockedCryptoGraphyClient(), this.mockedConfiguration.Object);
            var plainTextContent = "Its an awesome day!";
            var cipherText = encryptionService.Encrypt(plainTextContent);
            var decryptedText = encryptionService.Decrypt(cipherText);
            Assert.AreEqual(decryptedText, plainTextContent);
        }

        /// <summary>
        /// Tests for Encrypt method.
        /// </summary>
        [Test]
        public void EncryptDecryptTestInvalidInput()
        {
            EncryptionService encryptionService = new EncryptionService(this.mockedCryptographyClient.Object, this.mockedConfiguration.Object);
            var plainTextContent = "Its an awesome day!";
            var cipherText = "Gt8IuRCZT7wMsiII4J1+CA6uxnJKHKWne+xLbtJG8+8=";
            _ = Assert.Throws<ArgumentException>(() => encryptionService.Encrypt(plainTextContent));
            _ = Assert.Throws<ArgumentException>(() => encryptionService.Decrypt(cipherText));
        }
    }
}
