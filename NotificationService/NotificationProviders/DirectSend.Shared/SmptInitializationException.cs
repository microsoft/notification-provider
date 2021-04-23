// Copyright(c) Microsoft Corporation.
// Licensed under the MIT License.

namespace DirectSend
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// SmptInitializationException.
    /// </summary>
    /// <seealso cref="System.Exception" />
    public class SmptInitializationException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SmptInitializationException"/> class.
        /// </summary>
        public SmptInitializationException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SmptInitializationException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public SmptInitializationException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SmptInitializationException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (<see langword="Nothing" /> in Visual Basic) if no inner exception is specified.</param>
        public SmptInitializationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SmptInitializationException"/> class.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext" /> that contains contextual information about the source or destination.</param>
        protected SmptInitializationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
