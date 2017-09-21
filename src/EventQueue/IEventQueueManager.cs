using Glory.Services.Core.Config;
using Glory.Services.Core.EventQueue.EventBus.Abstractions;
using Glory.Services.Core.EventQueue.EventBus.Events;

namespace Glory.Services.Core.EventQueue
{
    public interface IEventQueueManager
    {
        void Publish(IntegrationEvent @event, ProviderLevel level = ProviderLevel.Normal);
        void Subscribe<T, TH>(string friendlyName)
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>;
        void SubscribeDynamic<TH>(string eventName, string friendlyName) where TH : IDynamicIntegrationEventHandler;
        void Unsubscribe<T, TH>(string friendlyName)
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>;
        void UnsubscribeDynamic<TH>(string eventName, string friendlyName) where TH : IDynamicIntegrationEventHandler;
    }
}