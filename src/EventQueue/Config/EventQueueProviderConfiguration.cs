using Glory.Services.Core.Config;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace Glory.Services.Core.EventQueue.Config
{
    public class EventQueueProviderConfiguration: ProviderConfiguration
    {
        public string AppName { get; private set; }

        public override void LoadValuesFromConfiguration(IConfigurationSection section)
        {
            base.LoadValuesFromConfiguration(section);
            AppName = section["appName"];
        }
    }
}
