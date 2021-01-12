﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Contracts
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    /// <summary>
    /// SingleValueExtendedProperty.
    /// </summary>
    public class SingleValueExtendedProperty
    {
        /// <summary>
        /// Gets or sets singleValueExtendedProperty.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets Value.
        /// </summary>
        public string Value { get; set; }
    }
}
