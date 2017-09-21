using Glory.Services.Core.EventQueue.EventBus;
using Glory.Services.Core.EventQueue.EventBus.Abstractions;
using Glory.Services.Core.EventQueue.ExternalSubscriptions.Config;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Glory.Services.Core.EventQueue.ExternalSubscriptions
{
    public class ExternalSubscriptionsManager : IExternalSubscriptionsManager
    {
        private readonly IEventQueueManager _eventQueueManager;
        public ExternalSubscriptionsManager(IEventQueueManager eventQueueManager)
        {
            _eventQueueManager = eventQueueManager;
        }

        public void RegisterAsIntegrationEvent()
        {
            var config = EventQueueConfiguration.GetConfig();
            foreach (var eventName in config.PublishedEvents.Keys)
            {
                RegisterAsIntegrationEvent(eventName);
            }
        }

        public void RegisterAsIntegrationEvent(string eventName)
        {
            _eventQueueManager.SubscribeDynamic<ExternalSubscriberHandler>(eventName, "External App Subscriber Handler");
        }

        public void RegisterPublicEventSubscriber(string eventName, SubscriberInfo subscriber, int thresholdSeconds = 0)
        {
            EventQueueConfiguration config = EventQueueConfiguration.GetConfig();
            var isNeedSave = false;
            if (!config.PublishedEvents.ContainsKey(eventName))
            {
                config.PublishedEvents.Add(eventName, new PublishedEvent()
                {
                    EventName = eventName,
                    Subscribers = subscriber.ID,
                    EventThresholdSeconds = thresholdSeconds
                });
                RegisterAsIntegrationEvent(eventName);
                isNeedSave = true;
            }
            else
            {
                string subscribers = config.PublishedEvents[eventName].Subscribers;
                if (string.IsNullOrEmpty(subscribers))
                {
                    config.PublishedEvents[eventName].Subscribers = subscriber.ID;
                    isNeedSave = true;
                }
                else if (subscribers.IndexOf(subscriber.ID) == -1)
                {
                    subscribers = string.Concat(subscribers, ";", subscriber.ID);
                    config.PublishedEvents[eventName].Subscribers = subscribers;
                    isNeedSave = true;
                }
            }
            if (!config.EventQueueSubscribers.ContainsKey(subscriber.ID))
            {
                config.EventQueueSubscribers.Add(subscriber.ID, subscriber);
                isNeedSave = true;
            }
            if (isNeedSave)
                config.Save();
        }

        public void AddPublicEvent(PublishedEvent publishedEvent)
        {
            EventQueueConfiguration config = EventQueueConfiguration.GetConfig();
            if (!config.PublishedEvents.ContainsKey(publishedEvent.EventName))
            {
                config.PublishedEvents.Add(publishedEvent.EventName, publishedEvent);
                RegisterAsIntegrationEvent(publishedEvent.EventName);
                config.Save();
            }
            else
                throw new ArgumentException($"{publishedEvent.EventName} already exists.");
        }

        public void AddEventSubscriber(string strEventName, string strSubscriberID)
        {
            EventQueueConfiguration config = EventQueueConfiguration.GetConfig();
            if (!config.PublishedEvents.ContainsKey(strEventName))
            {
                PublishedEvent publishedEvent = new PublishedEvent()
                {
                    EventName = strEventName,
                    Subscribers = strSubscriberID
                };
                config.PublishedEvents.Add(strEventName, publishedEvent);
                RegisterAsIntegrationEvent(publishedEvent.EventName);
                config.Save();
            }
            else
            {
                string subscribers = config.PublishedEvents[strEventName].Subscribers;
                if (string.IsNullOrEmpty(subscribers))
                {
                    config.PublishedEvents[strEventName].Subscribers = strSubscriberID;
                    config.Save();
                }
                else if (subscribers.IndexOf(strSubscriberID) == -1)
                {
                    subscribers = string.Concat(subscribers, ";", strSubscriberID);
                    config.PublishedEvents[strEventName].Subscribers = subscribers;
                    config.Save();
                }
            }
        }

        public void AddSubscriber(SubscriberInfo objSubscriberInfo)
        {
            EventQueueConfiguration config = EventQueueConfiguration.GetConfig();
            if (!config.EventQueueSubscribers.ContainsKey(objSubscriberInfo.ID))
            {
                config.EventQueueSubscribers.Add(objSubscriberInfo.ID, objSubscriberInfo);
                config.Save();
            }
            else
                throw new ArgumentException($"{objSubscriberInfo.ID} already exists.");
        }

        public void RemovePublicEventSubscriber(string strEventName, string strSubscriberID)
        {
            EventQueueConfiguration config = EventQueueConfiguration.GetConfig();
            if (config.PublishedEvents.ContainsKey(strEventName))
            {
                string subscribers = config.PublishedEvents[strEventName].Subscribers;
                if (subscribers != null)
                {
                    int index = subscribers.IndexOf(strSubscriberID);
                    if (index == 0)
                    {
                        subscribers = subscribers.Replace(strSubscriberID, "").TrimStart(new char[] { ';' });
                    }
                    else if (index > 0)
                    {
                        subscribers = subscribers.Replace(string.Concat(";", strSubscriberID), "");
                    }
                    config.PublishedEvents[strEventName].Subscribers = subscribers;
                    config.Save();
                }
            }
        }

        public void RemovePublicSubscriber(SubscriberInfo objSubscriberInfo)
        {
            EventQueueConfiguration config = EventQueueConfiguration.GetConfig();
            if (config.EventQueueSubscribers.ContainsKey(objSubscriberInfo.ID))
            {
                config.EventQueueSubscribers.Remove(objSubscriberInfo.ID);
                config.Save();
            }
        }

        public void UpdatePublicEvent(PublishedEvent publishedEvent)
        {
            EventQueueConfiguration config = EventQueueConfiguration.GetConfig();
            if (config.PublishedEvents.ContainsKey(publishedEvent.EventName))
            {
                config.PublishedEvents[publishedEvent.EventName] = publishedEvent;
                config.Save();
            }
        }

        public void UpdatePublicSubscriber(SubscriberInfo objSubscriberInfo)
        {
            EventQueueConfiguration config = EventQueueConfiguration.GetConfig();
            if (config.EventQueueSubscribers.ContainsKey(objSubscriberInfo.ID))
            {
                config.EventQueueSubscribers[objSubscriberInfo.ID] = objSubscriberInfo;
                config.Save();
            }
        }

        public PublishedEvent GetPublishedEvent(string eventName)
        {
            PublishedEvent publishedEvent = null;
            EventQueueConfiguration.GetConfig().PublishedEvents.TryGetValue(eventName, out publishedEvent);
            return publishedEvent;
        }

        public SubscriberInfo GetPublicSubscriberByID(string strSubscriberID)
        {
            SubscriberInfo item;
            EventQueueConfiguration config = EventQueueConfiguration.GetConfig();
            if (!config.EventQueueSubscribers.ContainsKey(strSubscriberID))
            {
                item = null;
            }
            else
            {
                item = config.EventQueueSubscribers[strSubscriberID];
            }
            return item;
        }

        public IEnumerable<SubscriberInfo> GetPublicSubscribersByEventName(string eventName)
        {
            IEnumerable<SubscriberInfo> items = null;
            EventQueueConfiguration config = EventQueueConfiguration.GetConfig();
            if (config.PublishedEvents.ContainsKey(eventName))
            {
                var subscriberIDs = config.PublishedEvents[eventName].Subscribers.Split(';');
                items = config.EventQueueSubscribers.Select(c => c.Value).Where(c => subscriberIDs.Contains(c.ID));
            }
            return items;
        }

        public IEnumerable<PublishedEvent> GetPublishedEvents()
        {
            var config = EventQueueConfiguration.GetConfig();
            return config.PublishedEvents.Select(c => c.Value);
        }

        public IEnumerable<SubscriberInfo> GetSubscribers()
        {
            var config = EventQueueConfiguration.GetConfig();
            return config.EventQueueSubscribers.Select(c => c.Value);
        }

    }
}
