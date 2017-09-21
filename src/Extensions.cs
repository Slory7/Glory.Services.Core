using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using Glory.Services.Core.EventQueue.ExternalSubscriptions;
using Glory.Services.Core.EventQueue.EventBus;
using Glory.Services.Core.DataCache.Config;
using Glory.Services.Core.EventQueue.Config;
using Glory.Services.Core.EventQueue.Providers;

namespace Glory.Services.Core
{
    public static class Extensions
    {
        private static IServiceProvider _serviceProvider;

        public static IServiceCollection RegisterGloryServices(this IServiceCollection services, IConfigurationRoot configuration)
        {
            services.AddMemoryCache();

            services.AddTransient<CacheProviderConfiguration>();
            var cacheProviderConfig = ProviderConfigurationHandler.Create<CacheProviderConfiguration>(configuration?.GetSection("caching"));
            cacheProviderConfig?.RegisterProvidersType(services);
            services.AddTransient((sp) => cacheProviderConfig);

            services.AddSingleton<IEventBusSubscriptionsManager, InMemoryEventBusSubscriptionsManager>();
            services.AddSingleton<IExternalSubscriptionsManager, ExternalSubscriptionsManager>();
            services.AddTransient<ExternalSubscriberHandler>();
            services.AddTransient<InsideEventQueueProvider>();
            var eventQueueProviderConfig = ProviderConfigurationHandler.Create<EventQueueProviderConfiguration>(configuration?.GetSection("eventQueue"));
            eventQueueProviderConfig?.RegisterProvidersType(services);
            services.AddTransient((sp) => eventQueueProviderConfig);

            var dataStoreProviderConfig = ProviderConfigurationHandler.Create<DataStoreProviderConfiguration>(configuration?.GetSection("dataStore"));
            dataStoreProviderConfig?.RegisterProvidersType(services);
            services.AddTransient((sp) => dataStoreProviderConfig);

            return services;
        }

        public static IServiceProvider ConfigureGloryProviders(this IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;

            var externalManger = GetService<IExternalSubscriptionsManager>();
            externalManger.RegisterAsIntegrationEvent();

            return serviceProvider;
        }

        public static T GetService<T>()
        {
            return _serviceProvider.GetService<T>();
        }

        public static object GetService(Type serviceType)
        {
            return _serviceProvider.GetService(serviceType);
        }

        public static object GetRequiredService(Type serviceType)
        {
            return _serviceProvider.GetRequiredService(serviceType);
        }
    }
}
