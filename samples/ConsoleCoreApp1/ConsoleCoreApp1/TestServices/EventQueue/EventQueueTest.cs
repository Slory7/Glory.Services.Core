using ConsoleCoreApp1.TestServices.EventQueue.Events;
using ConsoleCoreApp1.TestServices.EventQueue.Handlers;
using Glory.Services.Core.Config;
using Glory.Services.Core.EventQueue;
using Glory.Services.Core.EventQueue.ExternalSubscriptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ConsoleCoreApp1.TestServices.EventQueue
{
    public class EventQueueTest
    {
        private readonly IEventQueueManager _eventQueueManager;
        private readonly IExternalSubscriptionsManager _externalSubscriptionsManager;
        private readonly AppSettings _options;
        private readonly ILogger<EventQueueTest> _logger;

        public EventQueueTest(IEventQueueManager eventQueueManager
            , IExternalSubscriptionsManager externalSubscriptionsManager
            , IOptionsSnapshot<AppSettings> options
            , ILogger<EventQueueTest> logger
            )
        {
            _eventQueueManager = eventQueueManager;
            _externalSubscriptionsManager = externalSubscriptionsManager;
            _options = options.Value;
            _logger = logger;
        }

        public void Test()
        {
            _eventQueueManager.Subscribe<AppStartEvent, AppStartEventHandler>("App Start Event Handler");
            _eventQueueManager.SubscribeDynamic<DynamicEventHandler>("AppStartEvent", "Dynamic Event Handler");

            _eventQueueManager.Publish(new AppStartEvent() { AppName = _options.ApplicationName });

            //_externalSubscriptionsManager.RegisterPublicEventSubscriber("AppStartEvent", new SubscriberInfo()
            //{
            //    Address = "http://localhost/subscribemsg",
            //    Description = "App2 Event Message Subscriber",
            //    Name = "App2Subscriber",
            //},
            //thresholdSeconds: 5
            //);

            //_logger.LogWarning("new warning");

            _eventQueueManager.Publish(new AppStartEvent() { AppName = _options.ApplicationName });

        }
    }
}
