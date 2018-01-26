using Glory.Services.Core.Config;
using Glory.Services.Core.DataCache.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Glory.Services.Core.DataStore.Providers
{
    public abstract class DataStoreProvider
    {
        #region Private Members

        private static DataStoreProvider defaultProvider = null;
        private static Dictionary<ProviderLevel, DataStoreProvider> providerInstances =
            new Dictionary<ProviderLevel, DataStoreProvider>();

        private static Dictionary<int, DataStoreProvider> providerStageInstances =
            new Dictionary<int, DataStoreProvider>();

        #endregion

        #region constructors

        static DataStoreProvider()
        {
            var providerConfig = Extensions.GetService<DataStoreProviderConfiguration>();
            if (providerConfig == null)
            {
                throw new ArgumentNullException(nameof(providerConfig), "dataStore config section is missing.");
            }
            else
            {
                foreach (var objProvider in providerConfig.Providers.Values)
                {
                    if (objProvider.ProviderLevel != null || objProvider.Name == providerConfig.DefaultProvider)
                    {
                        Type objType = Type.GetType(objProvider.Type, true, true);

                        var objDataStoreProvider = (DataStoreProvider)Extensions.GetService(objType);

                        if (objProvider.Name == providerConfig.DefaultProvider)
                            defaultProvider = objDataStoreProvider;
                        if (objProvider.ProviderLevel != null)
                            providerInstances.Add((ProviderLevel)Enum.Parse(typeof(ProviderLevel), objProvider.ProviderLevel), objDataStoreProvider);
                        foreach (var attrName in objProvider.Attributes.AllKeys)
                        {
                            var attrValue = objProvider.Attributes[attrName];
                            var propInfo = objType.GetProperty(attrName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                            if (propInfo != null)
                            {
                                object objValue = Convert.ChangeType(attrValue, propInfo.PropertyType);
                                propInfo.SetValue(objDataStoreProvider, objValue, null);
                            }
                        }
                    }
                }
                //stages
                foreach (var objProvider in providerConfig.Stages.Values.OrderBy(c => c.StageSpanMinutes))
                {
                    Type objType = Type.GetType(objProvider.Type, true, true);

                    var objDataStoreProvider = (DataStoreProvider)Extensions.GetService(objType);

                    providerStageInstances.Add(objProvider.StageSpanMinutes, objDataStoreProvider);
                    foreach (var attrName in objProvider.Attributes.AllKeys)
                    {
                        var attrValue = objProvider.Attributes[attrName];
                        var propInfo = objType.GetProperty(attrName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                        if (propInfo != null)
                        {
                            object objValue = Convert.ChangeType(attrValue, propInfo.PropertyType);
                            propInfo.SetValue(objDataStoreProvider, objValue, null);
                        }
                    }
                }
            }
        }

        #endregion

        #region public static methods

        public static DataStoreProvider Instance(ProviderLevel Level)
        {
            if (providerInstances.ContainsKey(Level))
            {
                return providerInstances[Level];
            }
            return defaultProvider;
        }

        public static DataStoreProvider InstanceStage(int lastOnStageMinutes)
        {
            return providerStageInstances.First(c => c.Key >= lastOnStageMinutes || c.Key == -1).Value;
        }

        public static Dictionary<int, DataStoreProvider> InstanceStages()
        {
            return providerStageInstances;
        }

        #endregion

        #region public abstract methods

        public abstract IQueryable<T> Queryable<T>();

        public abstract Task Insert<T>(T doc);

        public abstract Task InsertMany<T>(IEnumerable<T> documents);

        public abstract Task<bool> Remove<T>(Expression<Func<T, bool>> filter);

        public abstract Task<bool> RemoveMany<T>(Expression<Func<T, bool>> filter);

        public abstract Task<bool> Update<T>(Expression<Func<T, bool>> filter, T doc);

        public abstract Task<T> IncrementField<T>(Expression<Func<T, bool>> filter, string field, int amount);

        public abstract Task InsertStageData<T>(int stageMinutes, T doc, string createdDateField);

        #endregion

    }
}
