using Glory.Services.Core.EventQueue.EventBus.Events;
using System;

namespace Glory.Services.Core.EventQueue.EventBus.Abstractions
{
    public interface IEventBus
    {
        void Subscribe<T, TH>(string friendlyName)
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>;
        void SubscribeDynamic<TH>(string eventName, string friendlyName)
            where TH : IDynamicIntegrationEventHandler;

        void UnsubscribeDynamic<TH>(string eventName)
            where TH : IDynamicIntegrationEventHandler;

        void Unsubscribe<T, TH>()
            where TH : IIntegrationEventHandler<T>
            where T : IntegrationEvent;

        void Publish(IntegrationEvent @event);
    }
}
