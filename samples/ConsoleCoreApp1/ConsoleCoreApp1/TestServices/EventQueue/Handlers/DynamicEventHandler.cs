using Glory.Services.Core.EventQueue.EventBus.Abstractions;
using Microsoft.Extensions.Logging;
using System;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleCoreApp1.TestServices.EventQueue.Handlers
{
    public class DynamicEventHandler : IDynamicIntegrationEventHandler
    {
        private readonly ILogger<DynamicEventHandler> _logger;

        public DynamicEventHandler(ILogger<DynamicEventHandler> logger)
        {
            _logger = logger;
        }

        public Task Handle(string eventName, dynamic eventObject)
        {
            return Task.Run(() =>
            {
                var appName = eventObject.AppName;
                _logger.LogInformation($"{appName} {eventName} Event Handled!");
            });
        }
    }
}
