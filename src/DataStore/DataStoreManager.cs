using Glory.Services.Core.Config;
using Glory.Services.Core.DataStore.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Glory.Services.Core.DataStore
{
    public class DataStoreManager : IDataStoreManager
    {
        public IQueryable<T> Queryable<T>(ProviderLevel level = ProviderLevel.Normal)
        {
            return DataStoreProvider.Instance(level).Queryable<T>();
        }

        public async Task Insert<T>(T doc, ProviderLevel level = ProviderLevel.Normal)
        {
            await DataStoreProvider.Instance(level).Insert(doc);
        }

        public async Task InsertMany<T>(IEnumerable<T> documents, ProviderLevel level = ProviderLevel.Normal)
        {
            await DataStoreProvider.Instance(level).InsertMany(documents);
        }

        public async Task<bool> Remove<T>(Expression<Func<T, bool>> filter, ProviderLevel level = ProviderLevel.Normal)
        {
            return await DataStoreProvider.Instance(level).Remove(filter);
        }

        public async Task<bool> RemoveMany<T>(Expression<Func<T, bool>> filter, ProviderLevel level = ProviderLevel.Normal)
        {
            return await DataStoreProvider.Instance(level).RemoveMany(filter);
        }

        public async Task<bool> Update<T>(Expression<Func<T, bool>> filter, T doc, ProviderLevel level = ProviderLevel.Normal)
        {
            return await DataStoreProvider.Instance(level).Update(filter, doc);
        }

        public IQueryable<T> StageQueryable<T>(int stageOnMinutes)
        {
            return DataStoreProvider.InstanceStage(stageOnMinutes).Queryable<T>();
        }

        public async Task InsertStageData<T>(T doc, string createdDateField = "CreatedDate")
        {
            foreach (var pair in DataStoreProvider.InstanceStages())
            {
                await pair.Value.InsertStageData(pair.Key, doc, createdDateField);
            }
        }

        public async Task UpdateStageData<T>(Expression<Func<T, bool>> filter, T doc)
        {
            foreach (var pair in DataStoreProvider.InstanceStages())
            {
                await pair.Value.Update(filter, doc);
            }
        }

        public async Task RemoveStageData<T>(Expression<Func<T, bool>> filter)
        {
            foreach (var pair in DataStoreProvider.InstanceStages())
            {
                await pair.Value.Remove(filter);
            }
        }

    }
}
