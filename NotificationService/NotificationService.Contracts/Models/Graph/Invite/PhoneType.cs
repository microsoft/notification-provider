// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Contracts.Models.Graph.Invite
{
    using System.Runtime.Serialization;

    /// <summary>
    /// PhoneType Enum.
    /// </summary>
    [DataContract]
    public enum PhoneType
    {
        /// <summary>
        /// PhoneType Home.
        /// </summary>
        [EnumMember(Value = "home")]
        Home,

        /// <summary>
        /// PhoneType Business.
        /// </summary>
        [EnumMember(Value = "business")]
        Business,

        /// <summary>
        /// PhoneType Mobile.
        /// </summary>
        [EnumMember(Value = "mobile")]
        Mobile,

        /// <summary>
        /// PhoneType Other.
        /// </summary>
        [EnumMember(Value = "other")]
        Other,

        /// <summary>
        /// PhoneType Assistant.
        /// </summary>
        [EnumMember(Value = "assistant")]
        Assistant,

        /// <summary>
        /// PhoneType HomeFax.
        /// </summary>
        [EnumMember(Value = "homeFax")]
        HomeFax,

        /// <summary>
        /// PhoneType BusinessFax.
        /// </summary>
        [EnumMember(Value = "businessFax")]
        BusinessFax,

        /// <summary>
        /// PhoneType OtherFax.
        /// </summary>
        [EnumMember(Value = "otherFax")]
        OtherFax,

        /// <summary>
        /// PhoneType Pager.
        /// </summary>
        [EnumMember(Value = "pager")]
        Pager,

        /// <summary>
        /// PhoneType Radio.
        /// </summary>
        [EnumMember(Value = "radio")]
        Radio,
    }
}
