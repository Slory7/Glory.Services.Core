using System.Collections.Generic;
using Glory.Services.Core.EventQueue.ExternalSubscriptions.Config;

namespace Glory.Services.Core.EventQueue.ExternalSubscriptions
{
    public interface IExternalSubscriptionsManager
    {
        void RegisterPublicEventSubscriber(string eventName, SubscriberInfo subscriber, int thresholdSeconds = 0);
        void AddEventSubscriber(string strEventName, string strSubscriberID);
        void AddPublicEvent(PublishedEvent publishedEvent);
        void AddSubscriber(SubscriberInfo objSubscriberInfo);
        SubscriberInfo GetPublicSubscriberByID(string strSubscriberID);
        PublishedEvent GetPublishedEvent(string eventName);
        IEnumerable<SubscriberInfo> GetPublicSubscribersByEventName(string eventName);
        IEnumerable<PublishedEvent> GetPublishedEvents();
        IEnumerable<SubscriberInfo> GetSubscribers();
        void RegisterAsIntegrationEvent();
        void RemovePublicEventSubscriber(string strEventName, string strSubscriberID);
        void RemovePublicSubscriber(SubscriberInfo objSubscriberInfo);
        void UpdatePublicEvent(PublishedEvent publishedEvent);
        void UpdatePublicSubscriber(SubscriberInfo objSubscriberInfo);
    }
}