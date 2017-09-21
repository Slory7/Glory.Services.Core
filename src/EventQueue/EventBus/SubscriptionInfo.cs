using System;

namespace Glory.Services.Core.EventQueue.EventBus
{
    public partial class InMemoryEventBusSubscriptionsManager : IEventBusSubscriptionsManager
    {
        public class SubscriptionInfo
        {
            public bool IsDynamic { get; }
            public Type HandlerType { get; }
            public string Name { get; }
            public SubscriptionInfo(bool isDynamic, Type handlerType, string name)
            {
                IsDynamic = isDynamic;
                HandlerType = handlerType;
                Name = name;
            }
        }
    }
}
