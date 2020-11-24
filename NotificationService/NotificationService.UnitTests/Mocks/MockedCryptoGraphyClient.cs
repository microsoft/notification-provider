// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.UnitTests.Mocks
{
    using System;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Azure.Core.Cryptography;

    /// <summary>
    /// MockedCryptoGraphyClient.
    /// </summary>
    /// <seealso cref="Azure.Core.Cryptography.IKeyEncryptionKey" />
    public class MockedCryptoGraphyClient : IKeyEncryptionKey
    {
        /// <summary>
        /// The aes.
        /// </summary>
        private readonly Aes aes;

        /// <summary>
        /// Initializes a new instance of the <see cref="MockedCryptoGraphyClient"/> class.
        /// </summary>
        public MockedCryptoGraphyClient()
        {
            this.aes = Aes.Create();
        }

        /// <inheritdoc/>
        public string KeyId => throw new NotImplementedException();

        /// <inheritdoc/>
        public byte[] UnwrapKey(string algorithm, ReadOnlyMemory<byte> encryptedKey, CancellationToken cancellationToken = default)
        {
            if (Encoding.UTF8.GetString(encryptedKey.ToArray()) == "EncryptedKey")
            {
                return this.aes.Key;
            }
            else if (Encoding.UTF8.GetString(encryptedKey.ToArray()) == "EncryptedIV")
            {
                return this.aes.IV;
            }
            else
            {
                return null;
            }
        }

        /// <inheritdoc/>
        public Task<byte[]> UnwrapKeyAsync(string algorithm, ReadOnlyMemory<byte> encryptedKey, CancellationToken cancellationToken = default) => throw new NotImplementedException();

        /// <inheritdoc/>
        public byte[] WrapKey(string algorithm, ReadOnlyMemory<byte> key, CancellationToken cancellationToken = default) => throw new NotImplementedException();

        /// <inheritdoc/>
        public Task<byte[]> WrapKeyAsync(string algorithm, ReadOnlyMemory<byte> key, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }
}
