using Glory.Services.Core.EventQueue.EventBus.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;
using Glory.Services.Core.EventQueue.EventBus.Events;
using Glory.Services.Core.EventQueue.EventBus;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Glory.Services.Core.EventQueue.Providers
{
    public class InsideEventQueueProvider : EventQueueProvider
    {
        #region Constructor

        private readonly IEventBusSubscriptionsManager _subsManager;
        private readonly ILogger<InsideEventQueueProvider> _logger;

        public InsideEventQueueProvider(IEventBusSubscriptionsManager subsManager
            , ILogger<InsideEventQueueProvider> logger)
        {
            _subsManager = subsManager ?? throw new ArgumentNullException(nameof(subsManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #endregion

        #region Abstract Method Implementation

        public override void Publish(IntegrationEvent @event)
        {
            var eventName = _subsManager.GetEventKey(@event);
            _subsManager.ProcessEvent(eventName, eventObject: @event);
        }

        public override void Start()
        {
            //throw new NotImplementedException();
        }

        public override void Subscribe(string eventName)
        {
            //throw new NotImplementedException();
        }

        public override void Unsubscribe(string eventName)
        {
            //throw new NotImplementedException();
        }

        #endregion

        #region  Private Methods        

        #endregion
    }
}

