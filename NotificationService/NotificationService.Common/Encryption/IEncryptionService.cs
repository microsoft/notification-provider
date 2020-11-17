// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Common.Encryption
{
    /// <summary>
    /// Common service to encrypt/decrypt sensitive data.
    /// </summary>
    public interface IEncryptionService
    {
        /// <summary>
        /// Encrypts the input text.
        /// </summary>
        /// <param name="input">string to be encrypted.</param>
        /// <returns>Cipher text.</returns>
        string Encrypt(string input);

        /// <summary>
        /// Decrypts the cipher text.
        /// </summary>
        /// <param name="cipherText">Cipher text to be decrypted.</param>
        /// <returns>Plain text.</returns>
        string Decrypt(string cipherText);
    }
}
