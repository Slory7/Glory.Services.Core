
#region Usings

using Glory.Services.Core.Config;
using Microsoft.Extensions.Configuration;
using System.Linq;

#endregion

namespace Glory.Services.Core.DataCache.Config
{
    internal class ProviderConfigurationHandler
    {
        public static T Create<T>(IConfigurationSection section)
            where T : ProviderConfiguration, new()
        {
            if (section == null || !section.GetChildren().Any())
                return null;
            var objProviderConfiguration = new T();
            objProviderConfiguration.LoadValuesFromConfiguration(section);
            return objProviderConfiguration;
        }
    }
}