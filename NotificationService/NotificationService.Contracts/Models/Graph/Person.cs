// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Contracts.Models.Graph
{
    using System.ComponentModel.DataAnnotations;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.Serialization;

    /// <summary>
    /// The <see cref="Person"/> class stores the person information.
    /// </summary>
    [ExcludeFromCodeCoverage]
    [DataContract]
    public class Person
    {
        /// <summary>
        /// Gets the system as <see cref="Person"/> object.
        /// </summary>
        /// <value>
        /// The system as <see cref="Person"/> object.
        /// </value>
        public static Person System => new Person { Name = "System", Email = "NA", ObjectIdentifier = "NA" };

        /// <summary>
        /// Gets all persons value.
        /// </summary>
        /// <value>
        /// All persons.
        /// </value>
        public static Person All => new Person { Name = "All", Email = "NA", ObjectIdentifier = "NA" };

        /// <summary>
        /// Gets or sets the person name.
        /// </summary>
        /// <value>
        /// The person name.
        /// </value>
        [DataMember(Name = "name")]
        [Required(ErrorMessage = "The name is mandatory", AllowEmptyStrings = false)]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the person email ID.
        /// </summary>
        /// <value>
        /// The person email ID.
        /// </value>
        [DataMember(Name = "email")]
        [Required(ErrorMessage = "The email is mandatory", AllowEmptyStrings = false)]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the person's object identifier.
        /// </summary>
        /// <value>
        /// The person's object identifier.
        /// </value>
        [DataMember(Name = "objectIdentifier")]
        [Required(ErrorMessage = "The object idenfier is mandatory", AllowEmptyStrings = false)]
        public string ObjectIdentifier { get; set; }
    }
}
