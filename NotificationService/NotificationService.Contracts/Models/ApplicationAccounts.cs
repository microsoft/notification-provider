// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Contracts
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Mapping of Application to Accounts.
    /// </summary>
    public class ApplicationAccounts
    {
        /// <summary>
        /// Gets or sets Application Name.
        /// </summary>
        public string ApplicationName { get; set; }

        /// <summary>
        /// Gets or sets Valid App IDs associated to the application.
        /// </summary>
        public string ValidAppIds { get; set; }

        /// <summary>
        /// Gets the valid App Ids list associated to the application.
        /// </summary>
        public IList<string> ValidAppIdsList => SplitToList(this.ValidAppIds);

        /// <summary>
        /// Gets or sets Accounts mapped to the Application.
        /// </summary>
        public List<AccountCredential> Accounts { get; set; }

        /// <summary>
        /// Gets or sets Email address which shall be set in the From address of the notifications.
        /// </summary>
        public string FromOverride { get; set; }

        /// <summary>
        /// Converts a comma-seperated string to a list of strings.
        /// </summary>
        /// <param name="valuesAsSingleString">String to be split.</param>
        /// <returns>List of strings.</returns>
        private static IList<string> SplitToList(string valuesAsSingleString)
        {
            var values = new List<string>();

            if (!string.IsNullOrWhiteSpace(valuesAsSingleString))
            {
                values.AddRange(valuesAsSingleString.Split(Common.Constants.SplitCharacter).Select(v => v.Trim()));
            }

            return values;
        }
    }
}
