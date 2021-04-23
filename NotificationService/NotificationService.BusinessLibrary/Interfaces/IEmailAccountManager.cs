// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.BusinessLibrary.Interfaces
{
    using System.Collections.Generic;
    using NotificationService.Contracts;

    /// <summary>
    /// IEmailAccountManager interface.
    /// </summary>
    public interface IEmailAccountManager
    {
        /// <summary>
        /// Method to fetch account from the acccounts provided.
        /// </summary>
        /// <param name="applicationName">Application Name to be used as filter.</param>
        /// <param name="applicationAccounts">List of applicationAccounts.</param>
        /// <returns><see cref="AccountCredential"/>.</returns>
        public AccountCredential FetchAccountToBeUsedForApplication(
            string applicationName, List<ApplicationAccounts> applicationAccounts);

        /// <summary>
        /// Increments index.
        /// </summary>
        public void IncrementIndex();
    }
}
