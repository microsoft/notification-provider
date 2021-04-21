// Copyright(c) Microsoft Corporation.
// Licensed under the MIT License.

namespace DirectSend
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// ISmtpClientPool.
    /// </summary>
    public interface ISmtpClientPool
    {
        /// <summary>
        /// Gets the end point.
        /// </summary>
        /// <value>
        /// The end point.
        /// </value>
        string EndPoint { get; }

        /// <summary>
        /// Gets the client.
        /// </summary>
        /// <param name="traceProperties">The trace properties.</param>
        /// <returns>A <see cref="IDSSmtpClient"/>.</returns>
        Task<IDSSmtpClient> GetClient(Dictionary<string, string> traceProperties);

        /// <summary>
        /// Returns the client.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="traceProperties">The trace properties.</param>
        /// <returns>A <see cref="Task"/>.</returns>
        Task ReturnClient(IDSSmtpClient client, Dictionary<string, string> traceProperties);
    }
}
