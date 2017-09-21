using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Glory.Services.Core.EventQueue.EventBus.Abstractions
{
    public interface IDynamicIntegrationEventHandler
    {
        Task Handle(string eventName, dynamic eventData);
    }
}
