// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Contracts.Entities.Web
{
    using System.Runtime.Serialization;

    /// <summary>
    /// The <see cref="NotificationDeliveryChannel"/> enumeration defines the delivery channel values.
    /// </summary>
    public enum NotificationDeliveryChannel
    {
        /// <summary>
        /// The web channel.
        /// </summary>
        [EnumMember(Value = "Web")]
        Web,

        /// <summary>
        /// The email channel.
        /// </summary>
        [EnumMember(Value = "Email")]
        Email,

        /// <summary>
        /// The action center channel.
        /// </summary>
        [EnumMember(Value = "ActionCenter")]
        ActionCenter,
    }
}
