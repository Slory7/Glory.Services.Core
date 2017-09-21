using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleCoreApp1.TestServices.EventQueue.Handlers
{
    internal static class HandlerExtensions
    {
        public static ServiceCollection RegisterHandlerTypes (this ServiceCollection services)
        {
            services
                .AddTransient<AppStartEventHandler>()
                .AddTransient<DynamicEventHandler>();

            return services;
        }
    }
}
