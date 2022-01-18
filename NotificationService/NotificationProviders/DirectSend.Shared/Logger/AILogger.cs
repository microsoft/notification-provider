// <copyright file="AILogger.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace NotificationProviders.Common.Logger
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Microsoft.ApplicationInsights;
    using Microsoft.ApplicationInsights.DataContracts;
    using Microsoft.ApplicationInsights.Extensibility;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;

    /// <summary>
    /// Application Insights Logger helper implementation.
    /// </summary>
    public class AILogger : ILogger, IDisposable
    {
        /// <summary>
        /// Application Insights telemetry client object to push telemetry events to AI.
        /// </summary>
        private readonly TelemetryClient telemetryClient;

        /// <summary>
        /// Configuration object to specify AI logging configurations like Severity level, component type etc.
        /// </summary>
        private readonly LoggingConfiguration loggingConfiguration;

        /// <summary>
        /// Instance of global telemetry configuration.
        /// </summary>
        private readonly TelemetryConfiguration appTelemetryConfiguration;

        private bool isDisposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="AILogger"/> class with logging configuration like Severity level, component type etc.
        /// </summary>
        /// <param name="configuration">Configuration like Severity level, component type etc.</param>
        public AILogger(LoggingConfiguration configuration)
            : this(configuration, null, null)
        {
            // This constructor just called the other overloaded constructor with null parameters.
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AILogger"/> class with logging configuration like Severity level, component type etc.
        /// </summary>
        /// <param name="configuration">Configuration like Severity level, component type etc.</param>
        /// <param name="initializers">Array of telemetry initializers.</param>
        public AILogger(LoggingConfiguration configuration, ITelemetryInitializer[] initializers)
            : this(configuration, null, initializers)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AILogger"/> class with logging configuration like Severity level, component type etc.
        /// </summary>
        /// <param name="configuration">Configuration like Severity level, component type etc.</param>
        /// <param name="telemetryConfiguration">Global telemetry configuration.</param>
        /// <param name="initializers">Array of telemetry initializers.</param>
        public AILogger(LoggingConfiguration configuration, TelemetryConfiguration telemetryConfiguration, ITelemetryInitializer[] initializers)
        {
            this.appTelemetryConfiguration = TelemetryConfiguration.CreateDefault();

            if (telemetryConfiguration != null)
            {
                this.appTelemetryConfiguration = telemetryConfiguration;
            }

            if (initializers != null && initializers.Length > 0 && this.appTelemetryConfiguration != null)
            {
                foreach (ITelemetryInitializer entry in initializers)
                {
                    var initializer = this.appTelemetryConfiguration.TelemetryInitializers.FirstOrDefault(
                        x => x.GetType().Name == entry.GetType().Name);
                    if (initializer != default(ITelemetryInitializer))
                    {
                        _ = this.appTelemetryConfiguration.TelemetryInitializers.Remove(initializer);
                    }

                    this.appTelemetryConfiguration.TelemetryInitializers.Add(entry);
                }
            }

            this.telemetryClient = new TelemetryClient(this.appTelemetryConfiguration);
            this.loggingConfiguration = configuration;
            this.SetEnvironmentValues();
        }

        /// <summary>
        /// Gets or sets the instrumentation key of AI.
        /// </summary>
        public string AiKey
        {
            get
            {
                return this.appTelemetryConfiguration.InstrumentationKey;
            }

            set
            {
                this.appTelemetryConfiguration.InstrumentationKey = value;
            }
        }

        /// <summary>
        /// This method is used to write the Custom Events. To track any events in your processing.
        /// </summary>
        /// <param name="eventName">Custom Event Name.</param>
        /// <param name="properties">custom properties, add more dimensions to this, so it will be easy to trace and query.</param>
        /// <param name="metrics">custom metrics, if you want to track any metrics of that event. For ex: duration.</param>
        /// <param name="eventCode">EventCode property in case you want to track as part of properties.</param>
        public void WriteCustomEvent(string eventName, IDictionary<string, string> properties = null, IDictionary<string, double> metrics = null, string eventCode = null)
        {
            properties = UpdateEventCode(properties, eventCode);
            this.telemetryClient.TrackEvent(eventName, properties, metrics);
        }

        /// <summary>
        /// This method is used to log exceptions along with the parameter values.
        /// </summary>
        /// <param name="exception">exception object.</param>
        /// <param name="properties">custom properties of the exception.</param>
        /// <param name="metrics">custom metrics of the exception.</param>
        /// <param name="eventCode">Any event code for exception.</param>
        /// <param name="expressionOfParameters">comma separated expressions of parameters Ex: () => ParameterVariable.</param>
        public void WriteException(Exception exception, IDictionary<string, string> properties = null, IDictionary<string, double> metrics = null, string eventCode = null, params Expression<Func<object>>[] expressionOfParameters)
        {
            var methodParameters = GetParameters(expressionOfParameters);
            if (methodParameters != null && methodParameters.Count > 0)
            {
                if (properties == null)
                {
                    properties = new Dictionary<string, string>();
                }

                foreach (var parameter in methodParameters)
                {
                    properties.Add(parameter.Key, parameter.Value);
                }
            }

            properties = UpdateEventCode(properties, eventCode);
            this.WriteException(exception, properties, metrics);
        }

        /// <summary>
        /// This method is used to log exceptions along with the parameter values.
        /// </summary>
        /// <param name="exception">exception object.</param>
        /// <param name="properties">custom properties of the exception.</param>
        /// <param name="metrics">custom metrics of the exception.</param>
        /// <param name="eventCode">Any event code for exception.</param>
        public void WriteException(Exception exception, IDictionary<string, string> properties = null, IDictionary<string, double> metrics = null, string eventCode = null)
        {
            properties = UpdateEventCode(properties, eventCode);
            this.telemetryClient.TrackException(exception, properties, metrics);
        }

        /// <summary>
        /// This method is used to Write Trace.
        /// </summary>
        /// <param name="message">message which should be traced. Give as detailed as you need.</param>
        /// <param name="traceLevel">severity level of this trace.</param>
        /// <param name="properties">custom properties, add more dimensions to this, so it will be easy to trace and query.</param>
        public void WriteTrace(string message, LogLevel traceLevel, IDictionary<string, string> properties = null)
        {
            var severityLevel = SeverityLevel.Critical;
            switch (traceLevel)
            {
                case LogLevel.Trace:
                    severityLevel = SeverityLevel.Verbose;
                    break;
                case LogLevel.Debug:
                    severityLevel = SeverityLevel.Verbose;
                    break;
                case LogLevel.Information:
                    severityLevel = SeverityLevel.Information;
                    break;
                case LogLevel.Warning:
                    severityLevel = SeverityLevel.Warning;
                    break;
                case LogLevel.Error:
                    severityLevel = SeverityLevel.Error;
                    break;
                case LogLevel.Critical:
                    severityLevel = SeverityLevel.Critical;
                    break;
                case LogLevel.None:
                    break;
                default:
                    break;
            }

            if (this.loggingConfiguration.IsTraceEnabled && severityLevel >= this.loggingConfiguration.TraceLevel)
            {
                this.telemetryClient.TrackTrace(message, severityLevel, properties);
            }
        }

        /// <summary>
        /// This method is used to Write Trace with Severity Level Information.
        /// </summary>
        /// <param name="message">message which should be traced. Give as detailed as you need.</param>
        /// <param name="properties">custom properties, add more dimensions to this, so it will be easy to trace and query.</param>
        public void TraceInformation(string message, IDictionary<string, string> properties = null)
        {
            if (this.loggingConfiguration.IsTraceEnabled && this.loggingConfiguration.TraceLevel <= SeverityLevel.Information)
            {
                this.telemetryClient.TrackTrace(message, SeverityLevel.Information, properties);
            }
        }

        /// <summary>
        /// This method is used to Write Trace with Severity Level Verbose.
        /// </summary>
        /// <param name="message">message which should be traced. Give as detailed as you need.</param>
        /// <param name="properties">custom properties, add more dimensions to this, so it will be easy to trace and query.</param>
        public void TraceVerbose(string message, IDictionary<string, string> properties = null)
        {
            if (this.loggingConfiguration.IsTraceEnabled && this.loggingConfiguration.TraceLevel <= SeverityLevel.Verbose)
            {
                this.telemetryClient.TrackTrace(message, SeverityLevel.Verbose, properties);
            }
        }

        /// <summary>
        /// This method is used to Write Trace with Severity Level Warning.
        /// </summary>
        /// <param name="message">message which should be traced. Give as detailed as you need.</param>
        /// <param name="properties">custom properties, add more dimensions to this, so it will be easy to trace and query.</param>
        public void TraceWarning(string message, IDictionary<string, string> properties = null)
        {
            if (this.loggingConfiguration.IsTraceEnabled && this.loggingConfiguration.TraceLevel <= SeverityLevel.Warning)
            {
                this.telemetryClient.TrackTrace(message, SeverityLevel.Warning, properties);
            }
        }

        /// <summary>
        /// This method is used to Write Trace with Severity Level Critical.
        /// </summary>
        /// <param name="message">message which should be traced. Give as detailed as you need.</param>
        /// <param name="properties">custom properties, add more dimensions to this, so it will be easy to trace and query.</param>
        public void TraceFatal(string message, IDictionary<string, string> properties = null)
        {
            if (this.loggingConfiguration.IsTraceEnabled && this.loggingConfiguration.TraceLevel <= SeverityLevel.Critical)
            {
                this.telemetryClient.TrackTrace(message, SeverityLevel.Critical, properties);
            }
        }

        /// <summary>
        /// This method is used to Write Trace with Severity Level Error.
        /// </summary>
        /// <param name="message">message which should be traced. Give as detailed as you need.</param>
        /// <param name="properties">custom properties, add more dimensions to this, so it will be easy to trace and query.</param>
        public void TraceError(string message, IDictionary<string, string> properties = null)
        {
            if (this.loggingConfiguration.IsTraceEnabled && this.loggingConfiguration.TraceLevel <= SeverityLevel.Error)
            {
                this.telemetryClient.TrackTrace(message, SeverityLevel.Error, properties);
            }
        }

        /// <summary>
        /// This method is used to Write Trace.
        /// </summary>
        /// <param name="verbosity">severity level of this trace.</param>
        /// <param name="message">message which should be traced. Give as detailed as you need.</param>
        /// <param name="properties">custom properties, add more dimensions to this, so it will be easy to trace and query.</param>
        public void Trace(SeverityLevel verbosity, string message, IDictionary<string, string> properties = null)
        {
            if (this.loggingConfiguration.IsTraceEnabled && verbosity >= this.loggingConfiguration.TraceLevel)
            {
                this.telemetryClient.TrackTrace(message, verbosity, properties);
            }
        }

        /// <summary>
        /// This method is used to track any metrics in your processing.
        /// </summary>
        /// <param name="metricName">Metric Name.</param>
        /// <param name="metricValue">numeric value of the metric.</param>
        /// <param name="properties">custom properties, add more dimensions to this, so it will be easy to trace and query.</param>
        public void WriteMetric(string metricName, double metricValue, IDictionary<string, string> properties = null)
        {
            this.telemetryClient.TrackMetric(metricName, metricValue, properties);
        }

        /// <summary>
        /// Flushes the in-memory buffer and any metrics being pre-aggregated.
        /// </summary>
        public void Flush()
        {
            this.telemetryClient.Flush();
        }

        /// <inheritdoc/>
        public void TrackDependency(string dependencyTypeName, string target, string dependencyName, string data, DateTimeOffset startTime, TimeSpan duration, string resultCode, bool success)
        {
            this.telemetryClient.TrackDependency(dependencyTypeName, target, dependencyName, data, startTime, duration, resultCode, success);
        }

        /// <inheritdoc/>
        public void TrackDependency(string dependencyTypeName, string dependencyName, string data, DateTimeOffset startTime, TimeSpan duration, bool success)
        {
            this.telemetryClient.TrackDependency(dependencyTypeName, dependencyName, data, startTime, duration, success);
        }

        /// <summary>
        /// Dispose utpmodule.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose.
        /// </summary>
        /// <param name="disposing">disposing.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (this.isDisposed)
            {
                return;
            }

            if (disposing)
            {
                this.appTelemetryConfiguration.Dispose();
            }

            this.isDisposed = true;
        }

        /// <summary>
        /// Updates the event code in properties.
        /// </summary>
        /// <param name="properties">properties dictionary.</param>
        /// <param name="eventCode">event code to be updated.</param>
        /// <returns>result properties after updating the event code.</returns>
        private static IDictionary<string, string> UpdateEventCode(IDictionary<string, string> properties, string eventCode)
        {
            if (!string.IsNullOrEmpty(eventCode))
            {
                if (properties == null)
                {
                    properties = new Dictionary<string, string>();
                }

                properties["EventCode"] = eventCode;
            }

            return properties;
        }

        /// <summary>
        /// Helper method to extract the members of the expression of parameters.
        /// </summary>
        /// <param name="expressionOfParameters">comma separated expressions of parameters Ex: () => ParameterVariable.</param>
        /// <returns>dictionary of properties.</returns>
        private static Dictionary<string, string> GetParameters(params Expression<Func<object>>[] expressionOfParameters)
        {
            var result = new Dictionary<string, string>();
            MemberExpression memberExpression;
            foreach (var parameterExpression in expressionOfParameters)
            {
                if (parameterExpression.Body != null)
                {
                    if (parameterExpression.Body is MemberExpression)
                    {
                        memberExpression = (MemberExpression)parameterExpression.Body;
                    }
                    else
                    {
                        memberExpression = ((UnaryExpression)parameterExpression.Body).Operand as MemberExpression;
                    }

                    if (memberExpression != null && memberExpression.Member != null)
                    {
                        ConstantExpression constantExpression = (ConstantExpression)memberExpression.Expression;
                        if (constantExpression != null)
                        {
                            var objectValue = ((FieldInfo)memberExpression.Member).GetValue(constantExpression.Value);
                            var objectJsonValue = JsonConvert.SerializeObject(objectValue);
                            result.Add(memberExpression.Member.Name, objectJsonValue);
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Helper method to set the environment name in the environment initializer.
        /// </summary>
        private void SetEnvironmentValues()
        {
            var initializer = this.appTelemetryConfiguration.TelemetryInitializers.SingleOrDefault(item => item.GetType() == typeof(EnvironmentInitializer)) as EnvironmentInitializer;
            if (initializer != null)
            {
                initializer.EnvironmentName = this.loggingConfiguration.EnvironmentName;
            }
        }
    }
}
