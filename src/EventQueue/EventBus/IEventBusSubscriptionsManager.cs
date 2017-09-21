using Glory.Services.Core.EventQueue.EventBus.Abstractions;
using Glory.Services.Core.EventQueue.EventBus.Events;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Glory.Services.Core.EventQueue.EventBus.InMemoryEventBusSubscriptionsManager;

namespace Glory.Services.Core.EventQueue.EventBus
{
    public interface IEventBusSubscriptionsManager
    {
        bool IsEmpty { get; }
        event EventHandler<string> OnEventRemoved;
        void AddDynamicSubscription<TH>(string eventName, string friendlyName)
           where TH : IDynamicIntegrationEventHandler;

        void AddSubscription<T, TH>(string friendlyName)
           where T : IntegrationEvent
           where TH : IIntegrationEventHandler<T>;

        void RemoveSubscription<T, TH>()
             where TH : IIntegrationEventHandler<T>
             where T : IntegrationEvent;
        void RemoveDynamicSubscription<TH>(string eventName)
            where TH : IDynamicIntegrationEventHandler;

        bool HasSubscriptionsForEvent<T>() where T : IntegrationEvent;
        bool HasSubscriptionsForEvent(string eventName);
        Type GetEventTypeByName(string eventName);
        void Clear();
        IEnumerable<SubscriptionInfo> GetHandlersForEvent<T>() where T : IntegrationEvent;
        IEnumerable<SubscriptionInfo> GetHandlersForEvent(string eventName);
        IEnumerable<string> GetAllEvents();
        string GetEventKey<T>();
        string GetEventKey(IntegrationEvent evt);
        Task ProcessEvent(string eventName, string message = null, IntegrationEvent eventObject = null);
    }
}