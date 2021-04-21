// Copyright(c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationProviders.Common.Logger
{
    using System.Collections.Generic;
    using Microsoft.ApplicationInsights.Channel;
    using Microsoft.ApplicationInsights.Extensibility;

    /// <summary>
    /// EnvironmentInitializer.
    /// </summary>
    /// <seealso cref="ITelemetryInitializer" />
    public class EnvironmentInitializer : ITelemetryInitializer
    {
        /// <summary>
        /// The environment key.
        /// </summary>
        public const string EnvironmentKey = "EnvironmentName";

        /// <summary>
        /// The service line1 key.
        /// </summary>
        public const string ServiceLine1Key = "ServiceOffering";

        /// <summary>
        /// The service line2 key.
        /// </summary>
        public const string ServiceLine2Key = "ServiceLine";

        /// <summary>
        /// The service line3 key.
        /// </summary>
        public const string ServiceLine3Key = "Service";

        /// <summary>
        /// The service line4 key.
        /// </summary>
        public const string ServiceLine4Key = "ComponentName";

        /// <summary>
        /// The component identifier key.
        /// </summary>
        public const string ComponentIdKey = "ComponentId";

        /// <summary>
        /// My application identifier.
        /// </summary>
        public const string MyAppId = "IctoId";

        /// <summary>
        /// Gets or sets plain string that names the environment of the component. Default value is: Production.
        /// IMPORTANT: The only valid value for this property in production is "Production".
        /// If this attribute value is something else other than "Production" data won't be pushed
        /// to production clusters of MSIT Telemetry Store.
        /// </summary>
        /// <value>
        /// The name of the environment.
        /// </value>
        public string EnvironmentName { get; set; }

        /// <summary>
        /// Gets or sets plain string, value contains Level 1 hierarchy of Service Tree (also known as Service Group).
        /// </summary>
        public string ServiceOffering { get; set; }

        /// <summary>
        /// Gets or sets plain string, value contains Level 2 hierarchy of Service Tree (also known as Team Group).
        /// </summary>
        public string ServiceLine { get; set; }

        /// <summary>
        /// Gets or sets plain string, value contains Level 3 hierarchy of Service Tree.
        /// </summary>
        public string Service { get; set; }

        /// <summary>
        /// Gets or sets plain string, value contains Level 4 hierarchy of Service Tree.
        /// </summary>
        public string ComponentName { get; set; }

        /// <summary>
        /// Gets or sets plain string, value contains Service Tree Component Id (Guid) value.
        /// </summary>
        public string ComponentId { get; set; }

        /// <summary>
        /// Gets or sets plain string, value contains ICTO ID for which telemetry is being traced.
        /// </summary>
        public string IctoId { get; set; }

#pragma warning disable CS3001 // Argument type is not CLS-compliant

        /// <inheritdoc/>
        public void Initialize(ITelemetry telemetry)
#pragma warning restore CS3001 // Argument type is not CLS-compliant
        {
            if (telemetry?.Context == null)
            {
                return;
            }
#pragma warning disable CS0618
            var properties = telemetry.Context.Properties;

            this.ValidateEnvironmentData();

            // environmentKey
            AddPropertySafely(EnvironmentKey, this.EnvironmentName, properties);

            // ServiceLine1Key
            AddPropertySafely(ServiceLine1Key, this.ServiceOffering, properties);

            // ServiceLine2Key
            AddPropertySafely(ServiceLine2Key, this.ServiceLine, properties);

            // ServiceLine3Key
            AddPropertySafely(ServiceLine3Key, this.Service, properties);

            // ServiceLine4Key
            AddPropertySafely(ServiceLine4Key, this.ComponentName, properties);

            // ComponentIdKey
            AddPropertySafely(ComponentIdKey, this.ComponentId, properties);

            // Icto Id
            AddPropertySafely(MyAppId, this.IctoId, properties);
        }

        private static void AddPropertySafely(string propertyName, string propertyValue, IDictionary<string, string> properties)
        {
            if (properties == null || propertyValue == null)
            {
                return;
            }

            if (properties.ContainsKey(propertyName))
            {
                properties[propertyName] = propertyValue.Trim();
            }
            else
            {
                properties.Add(propertyName, propertyValue.Trim());
            }
        }

        // ReSharper disable once UnusedParameter.Local
        private static void ThrowExceptionIfnull(string value, string paramName)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new System.ArgumentNullException(paramName);
            }
        }

        private void ValidateEnvironmentData()
        {
            ThrowExceptionIfnull(this.EnvironmentName, EnvironmentKey);
            ThrowExceptionIfnull(this.ServiceOffering, ServiceLine1Key);
            ThrowExceptionIfnull(this.ServiceLine, ServiceLine2Key);
            ThrowExceptionIfnull(this.Service, ServiceLine3Key);
            ThrowExceptionIfnull(this.ComponentName, ServiceLine4Key);
            ThrowExceptionIfnull(this.ComponentId, ComponentIdKey);
        }
    }
}
