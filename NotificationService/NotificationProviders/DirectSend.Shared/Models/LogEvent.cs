// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace DirectSend.Models
{
    using System;
    using System.Collections.Generic;

    public class LogEvent
    {
        public LogEvent()
        {
            this.Id = Guid.NewGuid().ToString();
        }

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

        public LogEvent(string id, string operationName, string operationId, string parentId = null, IDictionary<string, string> properties = null)
        {
            this.Id = id;
            this.OperationName = operationName;
            this.OperationId = operationId;
            this.ParentId = parentId;
            this.Properties = properties;
        }

        public string Id { get; set; }

        public string OperationName { get; set; }

        public string OperationId { get; set; }

        public string ParentId { get; set; }

        public IDictionary<string, string> Properties { get; set; }
    }
}
