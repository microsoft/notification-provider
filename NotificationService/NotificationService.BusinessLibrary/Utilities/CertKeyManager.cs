// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.BusinessLibrary.Utilities
{
    using System;
    using System.Security.Cryptography.X509Certificates;

    /// <summary>
    /// Helper class to provide cert related functionalities.
    /// </summary>
    public static class CertKeyManager
    {
        /// <summary>
        /// returns the certificate corresposnding to the input thumbprint and location.
        /// </summary>
        /// <param name="thumbprint">thumbprint.</param>
        /// <param name="location">location.</param>
        /// <returns>X509Certificate2.</returns>
        public static X509Certificate2 GetCertificate(string thumbprint, StoreLocation location)
        {
            X509Certificate2Collection certificates = null;

            certificates = Find(X509FindType.FindByThumbprint, thumbprint, false, location);
            if (certificates.Count == 0)
            {
                throw new ArgumentException($"No certificate was found for thumbprint {thumbprint} in the {location} store");
            }

            return new X509Certificate2(certificates[0]);
        }

        /// <summary>
        /// returns the certificate collection.
        /// </summary>
        /// <param name="findType">findType.</param>
        /// <param name="thumbprint">thumbprint.</param>
        /// <param name="validOnly">validOnly.</param>
        /// <param name="location">location.</param>
        /// <returns>X509Certificate2Collection.</returns>
        private static X509Certificate2Collection Find(X509FindType findType, string thumbprint, bool validOnly, StoreLocation location)
        {
            X509Store store = new X509Store(StoreName.My, location);
            try
            {
                store.Open(OpenFlags.ReadOnly);
                return store.Certificates.Find(findType, thumbprint, validOnly);
            }
            finally
            {
                if (store != null)
                {
                    store.Close();
                }
            }
        }
    }
}
