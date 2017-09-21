
#region Usings

using Microsoft.Extensions.Configuration;
using System.Collections.Specialized;
using System;
using System.Linq;
#endregion

namespace Glory.Services.Core.Config
{
    public class Provider
    {
        private readonly NameValueCollection _ProviderAttributes = new NameValueCollection();
        private readonly string _ProviderName;
        private readonly string _ProviderType;
        private readonly string _ProviderLevel;

        public Provider(IConfigurationSection section)
        {
            //Set the name of the provider
            _ProviderName = section["name"];

            //Set the type of the provider
            _ProviderType = section["type"];

            if (section["providerLevel"] != null)
                _ProviderLevel = section["providerLevel"];
 
            //Store all the attributes in the attributes bucket
            foreach (var kp in section.AsEnumerable().Where(c => c.Value != null))
            {
                var keyShortName = kp.Key.Split(':').Last();
                if (keyShortName != "name" && keyShortName != "type" && keyShortName != "providerLevel")
                {
                    _ProviderAttributes.Add(keyShortName, kp.Value);
                }
            }
        }

        public string Name
        {
            get
            {
                return _ProviderName;
            }
        }

        public string Type
        {
            get
            {
                return _ProviderType;
            }
        }

        public string ProviderLevel
        {
            get
            {
                return _ProviderLevel;
            }
        }

        public NameValueCollection Attributes
        {
            get
            {
                return _ProviderAttributes;
            }
        }
    }
}