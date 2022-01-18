// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Common.Utility
{
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Extensions class.
    /// </summary>
    public static class Extensions
    {
        private static readonly Regex WhiteSpaceRegex = new Regex(@"\s+");

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

        /// <summary>
        /// Retruns the formatted datetime string.
        /// </summary>
        /// <param name="dateTime"> DateTime Object.</param>
        /// <param name="formatter"> Datetime formatter string.</param>
        /// <returns>formatted datetime string.</returns>
#pragma warning disable IDE0031 // Use null propagation
#pragma warning disable CA1305 // Specify IFormatProvider
        public static string FormatDate(this DateTime dateTime, string formatter) => dateTime != null ? dateTime.ToString(formatter) : null;
#pragma warning restore CA1305 // Specify IFormatProvider
#pragma warning restore IDE0031 // Use null propagation

        /// <summary>
        /// Get a list of given type T from given string and split characater.
        /// </summary>
        /// <typeparam name="T"> Generic Type.</typeparam>
        /// <param name="str">input string.</param>
        /// <param name="splitChar">splitting char.</param>
        /// <returns>list of Given Type.</returns>
        public static IList<T> GetListFromString<T>(this string str, char splitChar)
        {
            if (string.IsNullOrEmpty(str))
            {
                return null;
            }

            string[] splitDays = str.Split(splitChar);
            IList<T> list = new List<T>();
            for (int i = 0; i < splitDays.Length; i++)
            {
                list.Add((T)Enum.Parse(typeof(T), splitDays[i].Trim(), true));
            }

            return list;
        }

        /// <summary>
        ///  Removes all the whitespaces from given string.
        /// </summary>
        /// <param name="value">input string.</param>
        /// <returns>string without whitespaces.</returns>
        public static bool HasWhitespaces(this string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            return WhiteSpaceRegex.IsMatch(value);
        }

        /// <summary>
        /// Validates email address.
        /// </summary>
        /// <param name="emailId">emailId string to validate.</param>
        /// <returns>Validation status of email.</returns>
        public static bool IsValidEmail(this string emailId)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(emailId);
                return addr.Address == emailId;
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch
#pragma warning restore CA1031 // Do not catch general exception types
            {
                return false;
            }
        }
    }
}
