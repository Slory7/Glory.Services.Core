using Glory.Services.Core.EventQueue.EventBus.Abstractions;
using System;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ConsoleCoreApp1.TestServices.EventQueue.Events;

namespace ConsoleCoreApp1.TestServices.EventQueue.Handlers
{
    public class AppStartEventHandler : IIntegrationEventHandler<AppStartEvent>
    {
        public Task Handle(AppStartEvent eventObject)
        {
            return Task.Run(() =>
            {
                var logger = Program.ServiceProvider.GetService<ILogger<AppStartEventHandler>>();
                logger.LogInformation(eventObject.AppName + " Start Event Handled!");
            });
        }
    }
}
