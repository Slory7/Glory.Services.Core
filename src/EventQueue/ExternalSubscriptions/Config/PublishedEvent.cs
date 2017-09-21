using System;
using System.Xml.Serialization;

namespace Glory.Services.Core.EventQueue.ExternalSubscriptions.Config
{
    public class PublishedEvent
    {
        public string EventName { get; set; }

        public string Subscribers { get; set; }

        public int EventThresholdSeconds { get; set; }

    }
}