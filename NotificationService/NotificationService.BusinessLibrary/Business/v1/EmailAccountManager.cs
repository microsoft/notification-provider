// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.BusinessLibrary
{
    using System.Collections.Generic;
    using NotificationService.BusinessLibrary.Interfaces;
    using NotificationService.Contracts;

    /// <summary>
    /// EmailAccountManager.
    /// </summary>
    public class EmailAccountManager : IEmailAccountManager
    {
        /// <summary>
        /// selectedIndex.
        /// </summary>
        private int selectedIndex = 0;

        /// <summary>
        /// increments the selectedindex.
        /// </summary>
        public void IncrementIndex()
        {
            this.selectedIndex = this.selectedIndex + 1;
        }

        /// <summary>
        /// FetchAccountToBeUsedForApplication.
        /// </summary>
        /// <param name="applicationName">applicationName.</param>
        /// <param name="applicationAccounts">applicationAccounts.</param>
        /// <returns> Tuple(AuthenticationHeaderValue, AccountCredential).</returns>
        public AccountCredential FetchAccountToBeUsedForApplication(string applicationName, List<ApplicationAccounts> applicationAccounts)
        {
            AccountCredential selectedAccountCredential = null;
            var accountsOfApplication = applicationAccounts?.Find(a => a.ApplicationName == applicationName)?.Accounts?.FindAll(acc => acc.IsEnabled);

            if (accountsOfApplication != null && accountsOfApplication.Count > 0)
            {
                this.selectedIndex = this.selectedIndex > accountsOfApplication.Count - 1 ? 0 : this.selectedIndex;
                int indexOfAccountToBeUsedRR = this.selectedIndex;
                selectedAccountCredential = accountsOfApplication[indexOfAccountToBeUsedRR];
            }

            return selectedAccountCredential;
        }
    }
}
