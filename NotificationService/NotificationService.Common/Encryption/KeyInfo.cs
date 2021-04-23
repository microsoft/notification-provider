// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Common.Encryption
{
    using System;
    using System.Security.Cryptography;

    /// <summary>
    /// Key information used by Encryption Service.
    /// </summary>
    public class KeyInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="KeyInfo"/> class.
        /// </summary>
        public KeyInfo()
        {
            using (var myAes = Aes.Create())
            {
                this.Key = myAes.Key;
                this.Iv = myAes.IV;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyInfo"/> class.
        /// </summary>
        /// <param name="key">Key to be used for the AES Alogrithm.</param>
        /// <param name="iv">Initial Vector to be used for the AES Algorithm.</param>
        public KeyInfo(string key, string iv)
        {
            this.Key = Convert.FromBase64String(key);
            this.Iv = Convert.FromBase64String(iv);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyInfo"/> class.
        /// </summary>
        /// <param name="key">Key to be used for the AES Alogrithm.</param>
        /// <param name="iv">Initial Vector to be used for the AES Algorithm.</param>
        public KeyInfo(byte[] key, byte[] iv)
        {
            this.Key = key;
            this.Iv = iv;
        }

        /// <summary>
        /// Gets Key used by AES Algorithm.
        /// </summary>
#pragma warning disable CA1819 // Properties should not return arrays
        public byte[] Key { get; }
#pragma warning restore CA1819 // Properties should not return arrays

        /// <summary>
        /// Gets Initial Vector used by AES Algorithm.
        /// </summary>
#pragma warning disable CA1819 // Properties should not return arrays
        public byte[] Iv { get; }
#pragma warning restore CA1819 // Properties should not return arrays
    }
}
