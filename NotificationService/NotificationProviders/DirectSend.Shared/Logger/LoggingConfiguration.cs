// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationProviders.Common.Logger
{
    using Microsoft.ApplicationInsights.DataContracts;

    /// <summary>
    /// Entity for maintaining the logging configuration.
    /// </summary>
    public class LoggingConfiguration
    {
        /// <summary>
        /// Gets or sets a value indicating whether the IsTraceEnabled property is true or false.
        /// </summary>
        public bool IsTraceEnabled { get; set; }

        /// <summary>
        /// Gets or sets the TraceLevel property.
        /// </summary>
        public SeverityLevel TraceLevel { get; set; }

        /// <summary>
        /// Gets or sets the EnvironmentName property.
        /// </summary>
        public string EnvironmentName { get; set; }
    }
}
