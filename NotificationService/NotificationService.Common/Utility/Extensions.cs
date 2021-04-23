// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Common.Utility
{
    using System;
    using System.Collections.Generic;

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
    }
}
