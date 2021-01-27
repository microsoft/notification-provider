// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Contracts.Models.Graph.Invite
{
    using System.Runtime.Serialization;

    /// <summary>
    /// Recurrence Model.
    /// </summary>
    [DataContract]
    public class Recurrence
    {
        /// <summary>
        /// Gets or Sets Pattern.
        /// </summary>
        [DataMember(Name = "pattern")]
        public RecurrencePattern Pattern { get; set; }

        /// <summary>
        /// Gets or Sets Range.
        /// </summary>
        [DataMember(Name = "range")]
        public RecurrenceRange Range { get; set; }
    }
}
