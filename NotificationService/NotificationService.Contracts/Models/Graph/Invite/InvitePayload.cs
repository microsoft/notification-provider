// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Contracts.Models.Graph.Invite
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    /// <summary>
    /// Graph Invite Payload.
    /// </summary>
    [DataContract]
    public class InvitePayload
    {
        /// <summary>
        /// Gets Or Sets Subject.
        /// </summary>
        [DataMember(Name = "subject", IsRequired = true)]
        public string Subject { get; set; }

        /// <summary>
        /// Gets Or Sets Body Content.
        /// </summary>
        [DataMember(Name = "body", IsRequired = true)]
        public MessageBody Body { get; set; }

        /// <summary>
        /// Gets Or Sets Meeting start DateTime.
        /// </summary>
        [DataMember(Name = "start", IsRequired = true)]
        public InviteDateTime Start { get; set; }

        /// <summary>
        /// Gets Or Sets Meeting end Datetime.
        /// </summary>
        [DataMember(Name = "end", IsRequired = true)]
        public InviteDateTime End { get; set; }

        /// <summary>
        /// Gets or Sets Location.
        /// </summary>
        [DataMember(Name = "location", IsRequired = true)]
        public Location Location { get; set; }

        /// <summary>
        /// Gets or Sets Attendees.
        /// </summary>
        [DataMember(Name = "attendees", IsRequired = true)]
        public IList<Attendee> Attendees { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether gets or Sets AllowNewTimeProposals.
        /// </summary>
        [DataMember(Name = "allowNewTimeProposals")]
        public bool AllowNewTimeProposals { get; set; }

        /// <summary>
        /// Gets or Sets TransactionId.
        /// </summary>
        [DataMember(Name = "transactionId")]
        public string TransactionId { get; set; }

        /// <summary>
        /// Gets or Sets Locations details.
        /// </summary>
        [DataMember(Name = "locations", IsRequired = false)]
        public IList<Location> Locations { get; set; }

        /// <summary>
        /// Gets or Sets Recurrence of Meeting invite.
        /// </summary>
        [DataMember(Name = "recurrence", IsRequired = false)]
        public Recurrence Recurrence { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether gets or Sets IsOnlineMeeting.
        /// </summary>
        [DataMember(Name = "isOnlineMeeting", IsRequired = true)]
        public bool IsOnlineMeeting { get; set; } = false;

        /// <summary>
        /// Gets or Sets OnlineMeetingProvider.
        /// </summary>
        [DataMember(Name = "onlineMeetingProvider", IsRequired = false)]
        public OnlineMeetingProviderType OnlineMeetingProvider { get; set; }

        /// <summary>
        /// Gets or Sets Meeting Organizer.
        /// </summary>
        [DataMember(Name = "organizer", IsRequired = true)]
        public Organizer Organizer { get; set; }

        /// <summary>
        /// Gets or Sets webLink (The URL to open the event in Outlook on the web).
        /// </summary>
        [DataMember(Name = "webLink")]
        public string WebLink { get; set; }

        /// <summary>
        /// Gets or Sets Importance of the meeitng invite.
        /// </summary>
        [DataMember(Name = "importance")]
        public ImportanceType Importance { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether gets or Sets the cancelled status of meeting invite.
        /// </summary>
        [DataMember(Name = "isCancelled")]
        public bool IsCancelled { get; set; } = false;

        /// <summary>
        /// Gets or Sets a value indicating whether gets or Sets as value indicating is reminder on.
        /// </summary>
        [DataMember(Name = "isReminderOn")]
        public bool IsReminderOn { get; set; } = false;

        /// <summary>
        /// Gets or Sets OnlineMeetingUrl (A URL for an online meeting).
        /// </summary>
        [DataMember(Name = "onlineMeetingUrl")]
        public string OnlineMeetingUrl { get; set; }

        /// <summary>
        /// Gets or Sets OnlineMeetingInfo.
        /// </summary>
        [DataMember(Name = "onlineMeetingInfo")]
        public OnlineMeetingInfo OnlineMeetingInfo { get; set; }

        /// <summary>
        /// Gets or Sets ReminderMinutesBeforeStart.
        /// </summary>
        [DataMember(Name = "reminderMinutesBeforeStart")]
        public int? ReminderMinutesBeforeStart { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether gets or Sets as value indicating whether the meeting invite is for all day.
        /// </summary>
        [DataMember(Name = "isAllDay")]
        public bool IsAllDay { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether gets or Sets as value indicating whether the meeting invite has attachments.
        /// </summary>
        [DataMember(Name = "hasAttachments")]
        public bool HasAttachments { get; set; }

        /// <summary>
        /// Gets or Sets ICallUid.
        /// </summary>
        [DataMember(Name = "iCalUId")]
        public string ICallUid { get; set; }
    }
}
