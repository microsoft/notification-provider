// <copyright file="ILogger.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace NotificationProviders.Common.Logger
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using Microsoft.ApplicationInsights.DataContracts;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// ILogger interface with the declaration of methods for logging.
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// This method is used to log exceptions along with the parameter values.
        /// </summary>
        /// <param name="exception">exception object.</param>
        /// <param name="properties">custom properties of the exception.</param>
        /// <param name="metrics">custom metrics of the exception.</param>
        /// <param name="eventCode">Any event code for exception.</param>
        /// <param name="expressionOfParameters">comma separated expressions of parameters Ex: () => ParameterVariable.</param>
        void WriteException(Exception exception, IDictionary<string, string> properties = null, IDictionary<string, double> metrics = null, string eventCode = null, params Expression<Func<object>>[] expressionOfParameters);

        /// <summary>
        /// This method is used to log exceptions along with the parameter values.
        /// </summary>
        /// <param name="exception">exception object.</param>
        /// <param name="properties">custom properties of the exception.</param>
        /// <param name="metrics">custom metrics of the exception.</param>
        /// <param name="eventCode">Any event code for exception.</param>
        void WriteException(Exception exception, IDictionary<string, string> properties = null, IDictionary<string, double> metrics = null, string eventCode = null);

        /// <summary>
        /// This method is used to Write Trace.
        /// </summary>
        /// <param name="message">message which should be traced. Give as detailed as you need.</param>
        /// <param name="traceLevel">severity level of this trace.</param>
        /// <param name="properties">custom properties, add more dimensions to this, so it will be easy to trace and query.</param>
        void WriteTrace(string message, LogLevel traceLevel, IDictionary<string, string> properties = null);

        /// <summary>
        /// This method is used to write the Custom Events. To track any events in your processing.
        /// </summary>
        /// <param name="eventName">Custom Event Name.</param>
        /// <param name="properties">custom properties, add more dimensions to this, so it will be easy to trace and query.</param>
        /// <param name="metrics">custom metrics, if you want to track any metrics of that event. For ex: duration.</param>
        /// <param name="eventCode">event code.</param>
        void WriteCustomEvent(string eventName, IDictionary<string, string> properties = null, IDictionary<string, double> metrics = null, string eventCode = null);

        /// <summary>
        /// This method is used to track any metrics in your processing.
        /// </summary>
        /// <param name="metricName">Metric Name.</param>
        /// <param name="metricValue">numeric value of the metric.</param>
        /// <param name="properties">custom properties, add more dimensions to this, so it will be easy to trace and query.</param>
        void WriteMetric(string metricName, double metricValue, IDictionary<string, string> properties = null);

        /// <summary>
        /// This method is used to Write Trace with Severity Level Information.
        /// </summary>
        /// <param name="message">message which should be traced. Give as detailed as you need.</param>
        /// <param name="properties">custom properties, add more dimensions to this, so it will be easy to trace and query.</param>
        void TraceInformation(string message, IDictionary<string, string> properties = null);

        /// <summary>
        /// This method is used to Write Trace with Severity Level Verbose.
        /// </summary>
        /// <param name="message">message which should be traced. Give as detailed as you need.</param>
        /// <param name="properties">custom properties, add more dimensions to this, so it will be easy to trace and query.</param>
        void TraceVerbose(string message, IDictionary<string, string> properties = null);

        /// <summary>
        /// This method is used to Write Trace with Severity Level Warning.
        /// </summary>
        /// <param name="message">message which should be traced. Give as detailed as you need.</param>
        /// <param name="properties">custom properties, add more dimensions to this, so it will be easy to trace and query.</param>
        void TraceWarning(string message, IDictionary<string, string> properties = null);

        /// <summary>
        /// This method is used to Write Trace with Severity Level Critical.
        /// </summary>
        /// <param name="message">message which should be traced. Give as detailed as you need.</param>
        /// <param name="properties">custom properties, add more dimensions to this, so it will be easy to trace and query.</param>
        void TraceFatal(string message, IDictionary<string, string> properties = null);

        /// <summary>
        /// This method is used to Write Trace with Severity Level Error.
        /// </summary>
        /// <param name="message">message which should be traced. Give as detailed as you need.</param>
        /// <param name="properties">custom properties, add more dimensions to this, so it will be easy to trace and query.</param>
        void TraceError(string message, IDictionary<string, string> properties = null);

        /// <summary>
        /// This method is used to Write Trace.
        /// </summary>
        /// <param name="verbosity">severity level of this trace.</param>
        /// <param name="message">message which should be traced. Give as detailed as you need.</param>
        /// <param name="properties">custom properties, add more dimensions to this, so it will be easy to trace and query.</param>
        void Trace(SeverityLevel verbosity, string message, IDictionary<string, string> properties = null);

        /// <summary>
        /// Flushes the in-memory buffer and any metrics being pre-aggregated.
        /// </summary>
        void Flush();

        /// <summary>
        /// Send information about an external dependency (outgoing call) in the application.
        /// </summary>
        /// <param name="dependencyTypeName">External dependency type. Very low cardinality value for logical grouping and  interpretation of fields. Examples are SQL, Azure table, and HTTP.</param>
        /// <param name="target">External dependency target.</param>
        /// <param name="dependencyName">Name of the command initiated with this dependency call. Low cardinality value. Examples are stored procedure name and URL path template.</param>
        /// <param name="data">Command initiated by this dependency call. Examples are SQL statement and HTTP. URL's with all query parameters.</param>
        /// <param name="startTime">The time when the dependency was called.</param>
        /// <param name="duration">The time taken by the external dependency to handle the call.</param>
        /// <param name="resultCode">Result code of dependency call execution.</param>
        /// <param name="success">True if the dependency call was handled successfully.</param>
        void TrackDependency(string dependencyTypeName, string target, string dependencyName, string data, DateTimeOffset startTime, TimeSpan duration, string resultCode, bool success);

        /// <summary>
        /// Send information about an external dependency (outgoing call) in the application.
        /// </summary>
        /// <param name="dependencyTypeName">External dependency type. Very low cardinality value for logical grouping and  interpretation of fields. Examples are SQL, Azure table, and HTTP.</param>
        /// <param name="dependencyName">Name of the command initiated with this dependency call. Low cardinality value.</param>
        /// <param name="data">Command initiated by this dependency call. Examples are SQL statement and HTTP. URL's with all query parameters.</param>
        /// <param name="startTime">The time when the dependency was called.</param>
        /// <param name="duration">The time taken by the external dependency to handle the call.</param>
        /// <param name="success">True if the dependency call was handled successfully.</param>
        void TrackDependency(string dependencyTypeName, string dependencyName, string data, DateTimeOffset startTime, TimeSpan duration, bool success);
    }
}
