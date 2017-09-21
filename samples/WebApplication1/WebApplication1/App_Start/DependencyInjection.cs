using Autofac;
using Autofac.Extensions.DependencyInjection;
using Autofac.Integration.Mvc;
using Autofac.Integration.WebApi;
using Glory.Services.Core;
using Glory.Services.Core.DataCache;
using Glory.Services.Core.DataStore;
using Glory.Services.Core.EventQueue;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;

namespace WebApplication1.App_Start
{
    public class DependencyInjection
    {
        public static void RegisterDependencyInjection()
        {
            var configurationBuilder = new ConfigurationBuilder()
                    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            IConfigurationRoot configuration = configurationBuilder.Build();

            var services = new ServiceCollection();
            ConfigureServices(services, configuration);

            var builder = new ContainerBuilder();

            //register others
            var config = GlobalConfiguration.Configuration;

            // Register your Web API controllers.
            builder.RegisterApiControllers(Assembly.GetExecutingAssembly());

            // Register your MVC controllers
            builder.RegisterControllers(Assembly.GetExecutingAssembly());

            builder.Populate(services);

            var container = builder.Build();
            var serviceProvider = new AutofacServiceProvider(container);

            ConfigureServicesProvider(serviceProvider);

            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
            config.DependencyResolver = new AutofacWebApiDependencyResolver(container);
        }
        private static void ConfigureServices(IServiceCollection services, IConfigurationRoot configuration)
        {
            //add logging
            services.AddLogging();

            //register Glory services
            services.RegisterGloryServices(configuration);

            services.AddSingleton<IDataCacheManager, DataCacheManager>();

            services.AddSingleton<IEventQueueManager, EventQueueManager>();

            //services.RegisterHandlerTypes();

            services.AddSingleton<IDataStoreManager, DataStoreManager>();

        }

        private static void ConfigureServicesProvider(IServiceProvider serviceProvider)
        {
            //configure Glory
            serviceProvider.ConfigureGloryProviders();

            serviceProvider
                .GetService<ILoggerFactory>()
                .AddNLog()
               .ConfigureNLog("config/nlog.config");
        }
    }
}