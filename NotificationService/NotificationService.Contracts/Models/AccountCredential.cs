// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Contracts
{
    /// <summary>
    /// Account Credential.
    /// </summary>
    public class AccountCredential
    {
        /// <summary>
        /// Gets or sets the Account Name.
        /// </summary>
        public string AccountName { get; set; }

        /// <summary>
        /// Gets or sets the Primary Password of Account.
        /// </summary>
        public string PrimaryPassword { get; set; }

        /// <summary>
        /// Gets or sets the Secondary Password of Account.
        /// </summary>
        public string SecondaryPassword { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this account is available for use or not.
        /// </summary>
        public bool IsEnabled { get; set; }
    }
}
