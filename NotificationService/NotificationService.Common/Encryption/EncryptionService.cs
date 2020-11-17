// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Common.Encryption
{
    using System;
    using System.IO;
    using System.Security.Cryptography;

    /// <summary>
    /// Common service to encrypt/decrypt sensitive data.
    /// </summary>
    public class EncryptionService : IEncryptionService
    {
        /// <summary>
        /// Key Information used for Encryption/Decryption.
        /// </summary>
        private readonly KeyInfo keyInfo;

        /// <summary>
        /// Initializes a new instance of the <see cref="EncryptionService"/> class.
        /// </summary>
        /// <param name="keyInfo">Key Information used for Encryption/Decryption.</param>
        public EncryptionService(KeyInfo keyInfo = null)
        {
            this.keyInfo = keyInfo;
        }

        /// <inheritdoc/>
        public string Encrypt(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                throw new ArgumentException("Payload to encrypt cannot be an empty string.", nameof(input));
            }

            var enc = this.EncryptStringToBytes_Aes(input);
            return Convert.ToBase64String(enc);
        }

        /// <inheritdoc/>
        public string Decrypt(string cipherText)
        {
            if (string.IsNullOrWhiteSpace(cipherText))
            {
                throw new ArgumentException("Payload to decrypt cannot be an empty string.", nameof(cipherText));
            }

            var cipherBytes = Convert.FromBase64String(cipherText);
            return this.DecryptStringFromBytes_Aes(cipherBytes);
        }

        /// <summary>
        /// Encrypts the input string using AES and returns back byte array.
        /// </summary>
        /// <param name="plainText">String to be encrypted.</param>
        /// <returns>Byte array of the encrypted content.</returns>
        private byte[] EncryptStringToBytes_Aes(string plainText)
        {
            if (plainText == null || plainText.Length <= 0)
            {
                throw new ArgumentNullException(nameof(plainText));
            }

            this.ValidateEncryptionParams();

            byte[] encrypted;

            using (var aesAlg = Aes.Create())
            {
                aesAlg.Key = this.keyInfo.Key;
                aesAlg.IV = this.keyInfo.Iv;

                var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
                using (var msEncrypt = new MemoryStream())
                {
                    using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (var swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }

                        encrypted = msEncrypt.ToArray();
                    }
                }
            }

            return encrypted;
        }

        /// <summary>
        /// Desrypts the input byte array using AES and returns back plain text.
        /// </summary>
        /// <param name="cipherText">Byte array to be decrypted.</param>
        /// <returns>Plain text.</returns>
        private string DecryptStringFromBytes_Aes(byte[] cipherText)
        {
            if (cipherText == null || cipherText.Length <= 0)
            {
                throw new ArgumentNullException(nameof(cipherText));
            }

            this.ValidateEncryptionParams();

            string plaintext;
            using (var aesAlg = Aes.Create())
            {
                aesAlg.Key = this.keyInfo.Key;
                aesAlg.IV = this.keyInfo.Iv;

                var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
                using (var msDecrypt = new MemoryStream(cipherText))
                {
                    using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (var srDecrypt = new StreamReader(csDecrypt))
                        {
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }

            return plaintext;
        }

        /// <summary>
        /// Validates the encryption key info and parameters.
        /// </summary>
        private void ValidateEncryptionParams()
        {
            if (this.keyInfo == null)
            {
                throw new ArgumentException(nameof(this.keyInfo));
            }

            if (this.keyInfo.Key == null || this.keyInfo.Key.Length <= 0)
            {
                throw new ArgumentException(nameof(this.keyInfo.Key));
            }

            if (this.keyInfo.Iv == null || this.keyInfo.Iv.Length <= 0)
            {
                throw new ArgumentException(nameof(this.keyInfo.Iv));
            }
        }
    }
}
