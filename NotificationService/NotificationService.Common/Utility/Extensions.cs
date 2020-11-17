// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Common.Utility
{
    /// <summary>
    /// Extensions class.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Base64Encode.
        /// </summary>
        /// <param name="value"> value of string.</param>
        /// <returns> A <see cref="string"/>.</returns>
        public static string Base64Encode(this string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }

            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(value);
            return System.Convert.ToBase64String(plainTextBytes);
        }
    }
}
