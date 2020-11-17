// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Common.Exceptions
{
    using System;

    /// <summary>
    /// The <see cref="NotificationServiceException"/> class represents exceptional behavior of notification service.
    /// </summary>
    /// <seealso cref="Exception" />
    public class NotificationServiceException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationServiceException"/> class.
        /// </summary>
        public NotificationServiceException()
            : this("Something went wrong!")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationServiceException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public NotificationServiceException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationServiceException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The instance of <see cref="Exception"/>.</param>
        public NotificationServiceException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
