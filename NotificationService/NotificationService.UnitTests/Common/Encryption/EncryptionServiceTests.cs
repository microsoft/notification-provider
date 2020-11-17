// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.UnitTests.Common.Encryption
{
    using System;
    using NotificationService.Common.Encryption;
    using NUnit.Framework;

    /// <summary>
    /// Tests for Encrypt/Decrypt methods of Common Encryption Service.
    /// </summary>
    public class EncryptionServiceTests
    {
        /// <summary>
        /// Initialization for the tests.
        /// </summary>
        [SetUp]
        public void Setup()
        {
        }

        /// <summary>
        /// Tests for Encrypt method.
        /// </summary>
        [Test]
        public void EncryptDecryptTestValidInput()
        {
            EncryptionService encryptionService = new EncryptionService(new KeyInfo());
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
            EncryptionService encryptionService = new EncryptionService();
            var plainTextContent = "Its an awesome day!";
            var cipherText = "Gt8IuRCZT7wMsiII4J1+CA6uxnJKHKWne+xLbtJG8+8=";
            _ = Assert.Throws<ArgumentException>(() => encryptionService.Encrypt(plainTextContent));
            _ = Assert.Throws<ArgumentException>(() => encryptionService.Decrypt(cipherText));
        }
    }
}
