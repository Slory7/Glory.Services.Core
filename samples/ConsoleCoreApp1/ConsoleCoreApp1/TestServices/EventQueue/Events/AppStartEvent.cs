using Glory.Services.Core.EventQueue.EventBus.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleCoreApp1.TestServices.EventQueue.Events
{
    public class AppStartEvent : IntegrationEvent
    {
        public string AppName { get; set; }
    }
}
