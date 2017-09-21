
#region Usings

using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

#endregion

namespace Glory.Services.Core.Config
{
    public class ProviderConfiguration
    {
        private readonly Dictionary<string, Provider> _Providers = new Dictionary<string, Provider>();
        private string _DefaultProvider;

        public string DefaultProvider
        {
            get
            {
                return _DefaultProvider;
            }
        }

        public Dictionary<string, Provider> Providers
        {
            get
            {
                return _Providers;
            }
        }      

        public  virtual void LoadValuesFromConfiguration(IConfigurationSection section)
        {
            //Get the default provider
            _DefaultProvider = section["defaultProvider"];

            //get providers section
            var providers = section.GetSection("providers");
            GetProviders(providers);           
        }

        private void GetProviders(IConfigurationSection section)
        {
            foreach (var provider in section.GetChildren())
            {
                Providers.Add(provider["name"], new Provider(provider));
            }
        }
        
        public virtual void RegisterProvidersType(IServiceCollection services)
        {
            foreach(var provider in _Providers.Values)
            {
                var objType = Type.GetType(provider.Type);
                services.AddTransient(objType);
            }
        }
    }
}