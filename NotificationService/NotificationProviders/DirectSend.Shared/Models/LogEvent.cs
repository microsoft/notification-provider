// <copyright file="LogEvent.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace DirectSend.Models
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// LogEvent Class.
    /// </summary>
    public class LogEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LogEvent"/> class.
        /// </summary>
        public LogEvent()
        {
            this.Id = Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogEvent"/> class.
        /// </summary>
        /// <param name="logEvent"> LogEcent object param.</param>
        public LogEvent(LogEvent logEvent)
        {
            this.Id = logEvent?.Id;
            this.OperationName = logEvent?.OperationName;
            this.OperationId = logEvent?.OperationId;
            this.ParentId = logEvent?.ParentId;
            if (logEvent != null && logEvent.Properties != null)
            {
                this.Properties = new Dictionary<string, string>(logEvent.Properties);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogEvent"/> class.
        /// </summary>
        /// <param name="logEvent"> LogEcent object param.</param>
        /// <param name="props">Additional properties for loggins in dictionary.</param>
        public LogEvent(LogEvent logEvent, IDictionary<string, string> props)
            : this(logEvent)
        {
            if (this.Properties != null)
            {
                if (props != null)
                {
                    foreach (var prop in props)
                    {
                        if (!this.Properties.ContainsKey(prop.Key))
                        {
                            this.Properties.Add(prop.Key, prop.Value);
                        }
                        else
                        {
                            this.Properties[prop.Key] = prop.Value;
                        }
                    }
                }
            }
            else if (props != null)
            {
                this.Properties = new Dictionary<string, string>(props);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogEvent"/> class.
        /// </summary>
        /// <param name="id"> Unique Id.</param>
        /// <param name="operationName">Operation Name for log event.</param>
        /// <param name="operationId">Operation Id for log event.</param>
        /// <param name="parentId">Parent Id for logging.</param>
        /// <param name="properties">Additional logging properties.</param>
        public LogEvent(string id, string operationName, string operationId, string parentId = null, IDictionary<string, string> properties = null)
        {
            this.Id = id;
            this.OperationName = operationName;
            this.OperationId = operationId;
            this.ParentId = parentId;
            this.Properties = properties;
        }

        /// <summary>
        /// Gets or Sets Id.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or Sets OperationName.
        /// </summary>
        public string OperationName { get; set; }

        /// <summary>
        /// Gets or Sets OperationId.
        /// </summary>
        public string OperationId { get; set; }

        /// <summary>
        /// Gets or Sets ParentId.
        /// </summary>
        public string ParentId { get; set; }

        /// <summary>
        /// Gets or Sets Properties.
        /// </summary>
#pragma warning disable CA2227 // Collection properties should be read only
        public IDictionary<string, string> Properties { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only
    }
}
