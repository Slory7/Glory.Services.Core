using ConsoleCoreApp1.TestServices.DataCache;
using ConsoleCoreApp1.TestServices.DataStore;
using ConsoleCoreApp1.TestServices.EventQueue;
using ConsoleCoreApp1.TestServices.EventQueue.Handlers;
using Glory.Services.Core;
using Glory.Services.Core.DataCache;
using Glory.Services.Core.DataStore;
using Glory.Services.Core.EventQueue;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ConsoleCoreApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables();

            IConfigurationRoot configuration = builder.Build();

            var services = new ServiceCollection();
            ConfigureServices(services, configuration);

            //build the provider;
            var serviceProvider = ServiceProvider = services.BuildServiceProvider();

            ConfigureServicesProvider(serviceProvider);

            TestServices(configuration, serviceProvider).Wait();
        }

        private static async Task TestServices(IConfigurationRoot configuration, IServiceProvider serviceProvider)
        {
            #region DataCache            

            var objDataCacheTest = serviceProvider.GetService<DataCacheTest>();
            objDataCacheTest.Test();

            #endregion

            #region EventQueue

            var objEventQueueTest = serviceProvider.GetService<EventQueueTest>();
            objEventQueueTest.Test();

            #endregion

            #region DataStore

            var objDataStoreTest = serviceProvider.GetService<DataStoreTest>();

            await objDataStoreTest.Test();

            #endregion

            Console.Read();
        }

        public static IServiceProvider ServiceProvider { get; private set; }

        private static void ConfigureServices(IServiceCollection services, IConfigurationRoot configuration)
        {
            services.Configure<AppSettings>(configuration);

            //add logging
            services.AddLogging();

            //register Glory services
            services.RegisterGloryServices(configuration);

            services.AddSingleton<IDataCacheManager, DataCacheManager>();

            services.AddSingleton<IEventQueueManager, EventQueueManager>();        

            services.RegisterHandlerTypes();

            services.AddSingleton<IDataStoreManager, DataStoreManager>();

            services.AddTransient<DataCacheTest>();
            services.AddTransient<EventQueueTest>();
            services.AddTransient<DataStoreTest>();
        }

        private static void ConfigureServicesProvider(IServiceProvider serviceProvider)
        {
            //configure Glory
            serviceProvider.ConfigureGloryProviders();

            serviceProvider
                .GetService<ILoggerFactory>()
                .AddConsole(LogLevel.Trace, true)
                .AddNLog()
               .ConfigureNLog("config/nlog.config");
        }
    }
}
