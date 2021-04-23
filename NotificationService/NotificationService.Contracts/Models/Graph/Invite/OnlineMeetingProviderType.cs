// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Contracts.Models.Graph.Invite
{
    using System.Runtime.Serialization;

    /// <summary>
    /// OnlineMeetingProviderType Model.
    /// </summary>
    [DataContract]
    public enum OnlineMeetingProviderType
    {
        /// <summary>
        /// OnlineMeetingProviderType TeamsForBusiness.
        /// </summary>
        [EnumMember(Value = "teamsForBusiness")]
        TeamsForBusiness,

        /// <summary>
        /// OnlineMeetingProviderType SkypeForBusiness.
        /// </summary>
        [EnumMember(Value = "skypeForBusiness")]
        SkypeForBusiness,

        /// <summary>
        /// OnlineMeetingProviderType SkypeForConsumer.
        /// </summary>
        [EnumMember(Value = "skypeForConsumer")]
        SkypeForConsumer,
    }
}
