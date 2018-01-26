using Glory.Services.Core.Config;
using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Glory.Services.Core.EventQueue.EventBus.Abstractions;
using Glory.Services.Core.EventQueue.EventBus.Events;
using Glory.Services.Core.EventQueue.EventBus;

namespace Glory.Services.Core.EventQueue
{
    public class EventQueueManager : IEventQueueManager
    {
        #region Constructor

        private readonly IEventBusSubscriptionsManager _subsManager;
        private readonly ILogger _logger;
        public EventQueueManager(
            IEventBusSubscriptionsManager subsManager
            , ILogger<EventQueueManager> logger
            )
        {
            _subsManager = subsManager ?? throw new ArgumentNullException(nameof(subsManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #endregion

        #region Public Properties


        #endregion

        #region Public Methods

        public void Publish(IntegrationEvent @event, ProviderLevel level)
        {
            var eventName = _subsManager.GetEventKey(@event);
            _logger.LogInformation($"Publish Start:{eventName}, ID:{@event.Id}");

            Providers.EventQueueProvider.Instance(level).Publish(@event);

            _logger.LogInformation($"Publish End:{eventName}, ID:{@event.Id}");
        }

        public void Subscribe<T, TH>(string friendlyName)
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>
        {
            _logger.LogInformation($"Subscribe Start: {friendlyName}");

            var eventName = _subsManager.GetEventKey<T>();
            foreach (var provider in Providers.EventQueueProvider.Instances())
            {
                provider.Subscribe(eventName);
            }

            _subsManager.AddSubscription<T, TH>(friendlyName);

            _logger.LogInformation($"Subscribe End: {friendlyName}");
        }

        public void SubscribeDynamic<TH>(string eventName, string friendlyName) where TH : IDynamicIntegrationEventHandler
        {
            _logger.LogInformation($"SubscribeDynamic Start: {friendlyName}");

            foreach (var provider in Providers.EventQueueProvider.Instances())
            {
                provider.Subscribe(eventName);
            }

            _subsManager.AddDynamicSubscription<TH>(eventName, friendlyName);

            _logger.LogInformation($"SubscribeDynamic End: {friendlyName}");
        }

        public void Unsubscribe<T, TH>(string friendlyName)
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>
        {
            _logger.LogInformation($"Unsubscribe Start: {friendlyName}");

            var eventName = _subsManager.GetEventKey<T>();
            foreach (var provider in Providers.EventQueueProvider.Instances())
            {
                provider.Unsubscribe(eventName);
            }

            _subsManager.RemoveSubscription<T, TH>();

            _logger.LogInformation($"Unsubscribe End: {friendlyName}");
        }

        public void UnsubscribeDynamic<TH>(string eventName, string friendlyName) where TH : IDynamicIntegrationEventHandler
        {
            _logger.LogInformation($"UnsubscribeDynamic Start: {friendlyName}");

            foreach (var provider in Providers.EventQueueProvider.Instances())
            {
                provider.Unsubscribe(eventName);
            }

            _subsManager.RemoveDynamicSubscription<TH>(eventName);

            _logger.LogInformation($"UnsubscribeDynamic End: {friendlyName}");
        }

        #endregion

    }
}