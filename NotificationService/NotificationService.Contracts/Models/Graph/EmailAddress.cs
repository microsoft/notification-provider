// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Contracts
{
    using System.ComponentModel.DataAnnotations;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.Serialization;

    /// <summary>
    /// Email Address wrapper entity for MS Graph.
    /// </summary>
    [ExcludeFromCodeCoverage]
    [DataContract]
    public class EmailAddress
    {
        /// <summary>
        /// Gets or sets email address.
        /// </summary>
        [DataMember(Name = "address")]
        [Required(ErrorMessage = "Email address is mandatory for recipient of email notifications.")]
        public string Address { get; set; }

        /// <summary>
        /// Gets or sets name of the person associated to the email address.
        /// </summary>
        [DataMember(Name = "name")]
        public string Name { get; set; }
    }
}
