// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace DirectSend.Models.Configurations
{
    /// <summary>
    /// SendAccountConfiguration
    /// </summary>
    public class SendAccountConfiguration
    {
        /// <summary>
        /// Gets or sets the address.
        /// </summary>
        /// <value>
        /// The address.
        /// </value>
        public string Address { get; set; }

        /// <summary>
        /// Gets or sets the display name.
        /// </summary>
        /// <value>
        /// The display name.
        /// </value>
        public string FromAddressDisplayName { get; set; }
    }
}
