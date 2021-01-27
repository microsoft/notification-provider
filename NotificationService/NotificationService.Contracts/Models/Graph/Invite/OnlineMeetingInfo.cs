// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Contracts.Models.Graph.Invite
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    /// <summary>
    /// OnlineMeetinInfo Model.
    /// </summary>
    [DataContract]
    public class OnlineMeetingInfo
    {
        /// <summary>
        /// Gets or Sets ConferenceId.
        /// </summary>
        [DataMember(Name = "conferenceId")]
        public string ConferenceId { get; set; }

        /// <summary>
        /// Gets or Sets JoinUrl.
        /// </summary>
        [DataMember(Name = "joinUrl")]
        public string JoinUrl { get; set; }

        /// <summary>
        /// Gets or Sets Phones.
        /// </summary>
        [DataMember(Name = "phones")]
        public IList<Phone> Phones { get; set; }

        /// <summary>
        /// Gets or Sets QuickDial.
        /// </summary>
        [DataMember(Name = "quickDial")]
        public string QuickDial { get; set; }

        /// <summary>
        /// Gets or Sets TollFreeNumbers.
        /// </summary>
        [DataMember(Name = "tollFreeNumbers")]
        public IList<string> TollFreeNumbers { get; set; }

        /// <summary>
        /// Gets or Sets TollNumber.
        /// </summary>
        [DataMember(Name = "tollNumber")]
        public string TollNumber { get; set; }
    }
}
