using Glory.Services.Core.EventQueue.EventBus.Events;
using System.Threading.Tasks;

namespace Glory.Services.Core.EventQueue.EventBus.Abstractions
{
    public interface IIntegrationEventHandler<in TIntegrationEvent> : IIntegrationEventHandler 
        where TIntegrationEvent: IntegrationEvent
    {
        Task Handle(TIntegrationEvent eventObject);
    }

    public interface IIntegrationEventHandler
    {
    }
}
