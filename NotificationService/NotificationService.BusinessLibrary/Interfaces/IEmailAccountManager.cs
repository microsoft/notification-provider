// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.BusinessLibrary.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading.Tasks;
    using NotificationService.Contracts;

    /// <summary>
    /// IEmailAccountManager interface.
    /// </summary>
    public interface IEmailAccountManager
    {
        public AccountCredential FetchAccountToBeUsedForApplication(
            string applicationName, List<ApplicationAccounts> applicationAccounts);

        public void IncrementIndex();
    }
}
