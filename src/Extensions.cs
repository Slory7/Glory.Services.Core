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
        private static IServiceProvider ServiceProvider
        {
            get
            {
                if (_serviceProvider == null)
                    throw new ArgumentNullException(nameof(_serviceProvider), "You should call ConfigureGloryProviders method.");
                return _serviceProvider;
            }
            set { _serviceProvider = value; }
        }

        public static IServiceCollection RegisterGloryServices(this IServiceCollection services, IConfiguration configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            services.AddTransient<CacheProviderConfiguration>();
            var cacheProviderConfig = ProviderConfigurationHandler.Create<CacheProviderConfiguration>(configuration.GetSection("caching"));
            if (cacheProviderConfig != null)
            {
                services.AddMemoryCache();
                cacheProviderConfig.RegisterProvidersType(services);
                services.AddTransient((sp) => cacheProviderConfig);
            }

            var eventQueueProviderConfig = ProviderConfigurationHandler.Create<EventQueueProviderConfiguration>(configuration.GetSection("eventQueue"));
            if (eventQueueProviderConfig != null)
            {
                services.AddSingleton<IEventBusSubscriptionsManager, InMemoryEventBusSubscriptionsManager>();
                services.AddSingleton<IExternalSubscriptionsManager, ExternalSubscriptionsManager>();
                services.AddTransient<ExternalSubscriberHandler>();
                services.AddTransient<InsideEventQueueProvider>();
                eventQueueProviderConfig.RegisterProvidersType(services);
                services.AddTransient((sp) => eventQueueProviderConfig);
            }

            var dataStoreProviderConfig = ProviderConfigurationHandler.Create<DataStoreProviderConfiguration>(configuration.GetSection("dataStore"));
            if (dataStoreProviderConfig != null)
            {
                dataStoreProviderConfig.RegisterProvidersType(services);
                services.AddTransient((sp) => dataStoreProviderConfig);
            }

            return services;
        }

        public static IServiceProvider ConfigureGloryProviders(this IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;

            var externalManger = TryGetService<IExternalSubscriptionsManager>();
            externalManger?.RegisterAsIntegrationEvent();

            return serviceProvider;
        }

        public static T GetService<T>() where T : class
        {
            return ServiceProvider.GetService<T>();
        }

        public static T TryGetService<T>() where T : class
        {
            object obj = null;
            try
            {
                obj = GetService(typeof(T));
            }
            catch
            {
            }
            return obj as T;
        }

        public static object GetService(Type serviceType)
        {
            return ServiceProvider.GetService(serviceType);
        }

        public static object GetRequiredService(Type serviceType)
        {
            return ServiceProvider.GetRequiredService(serviceType);
        }
    }
}
