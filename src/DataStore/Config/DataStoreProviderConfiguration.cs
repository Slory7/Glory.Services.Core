using Glory.Services.Core.Config;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Glory.Services.Core.DataCache.Config
{
    public class DataStoreProviderConfiguration: ProviderConfiguration
    {
        private readonly Dictionary<string, DSProvider> _Stages = new Dictionary<string, DSProvider>();
        public Dictionary<string, DSProvider> Stages
        {
            get
            {
                return _Stages;
            }
        }
        public override void LoadValuesFromConfiguration(IConfigurationSection section)
        {
            base.LoadValuesFromConfiguration(section);

            //get stages section
            var stages = section.GetSection("stages");
            if (stages != null)
                GetStages(stages);
        }

        private void GetStages(IConfigurationSection section)
        {
            foreach (var provider in section.GetChildren())
            {
                Stages.Add(provider["name"], new DSProvider(provider));
            }
        }

        public override void RegisterProvidersType(IServiceCollection services)
        {
            base.RegisterProvidersType(services);

            foreach (var provider in _Stages.Values)
            {
                var objType = Type.GetType(provider.Type);
                services.AddTransient(objType);
            }
        }

    }
}
