using Glory.Services.Core.Config;
using Glory.Services.Core.EventQueue.Config;
using Glory.Services.Core.EventQueue.EventBus.Abstractions;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using Glory.Services.Core.EventQueue.EventBus.Events;

namespace Glory.Services.Core.EventQueue.Providers
{
    public abstract class EventQueueProvider
    {
        #region Private Members

        private static EventQueueProvider defaultEventQueueProvider = null;
        private static Dictionary<ProviderLevel, EventQueueProvider> providerInstances =
            new Dictionary<ProviderLevel, EventQueueProvider>();

        #endregion

        #region Protected Properties      

        #endregion

        #region Constructors

        static EventQueueProvider()
        {
            var providerConfig = Extensions.GetService<EventQueueProviderConfiguration>();
            if (providerConfig == null)
            {
                defaultEventQueueProvider = Extensions.GetService<InsideEventQueueProvider>();
            }
            else
            {
                foreach (Provider objProvider in providerConfig.Providers.Values)
                {
                    if (objProvider.ProviderLevel != null || objProvider.Name == providerConfig.DefaultProvider)
                    {
                        Type objType = Type.GetType(objProvider.Type, true, true);

                        var objEventQueueProvider = (EventQueueProvider)Extensions.GetService(objType);

                        if (objProvider.Name == providerConfig.DefaultProvider)
                            defaultEventQueueProvider = objEventQueueProvider;
                        if (objProvider.ProviderLevel != null)
                            providerInstances.Add((ProviderLevel)Enum.Parse(typeof(ProviderLevel), objProvider.ProviderLevel), objEventQueueProvider);

                        var appNamePropInfo = objType.GetProperty("AppName", BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                        if (appNamePropInfo != null)
                            appNamePropInfo.SetValue(objEventQueueProvider, providerConfig.AppName, null);

                        foreach (var attrName in objProvider.Attributes.AllKeys)
                        {
                            var attrValue = objProvider.Attributes[attrName];
                            var propInfo = objType.GetProperty(attrName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                            if (propInfo != null)
                            {
                                object objValue = Convert.ChangeType(attrValue, propInfo.PropertyType);
                                propInfo.SetValue(objEventQueueProvider, objValue, null);
                            }
                        }
                        objEventQueueProvider.Start();
                    }
                }
            }
        }

        #endregion

        #region Shared/Static Methods

        /// <summary>
        /// Instances of provider.
        /// </summary>
        /// <returns>The Implemments provider of system defind in config.</returns>
        public static EventQueueProvider Instance(ProviderLevel Level)
        {
            if (providerInstances.ContainsKey(Level))
            {
                return providerInstances[Level];
            }
            return defaultEventQueueProvider;
        }
        public static IEnumerable<EventQueueProvider> Instances()
        {
            if (providerInstances.Count > 0)
            {
                return providerInstances.Select(c => c.Value);
            }
            return new List<EventQueueProvider>() { defaultEventQueueProvider };
        }
        #endregion

        #region Public Methods

        public abstract void Start();

        public abstract void Publish(IntegrationEvent @event);

        public abstract void Subscribe(string eventName);

        public abstract void Unsubscribe(string eventName);

        #endregion

    }
}
