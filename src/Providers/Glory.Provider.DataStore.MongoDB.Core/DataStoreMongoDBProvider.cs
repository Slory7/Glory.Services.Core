using Glory.Services.Core.Config;
using Glory.Services.Core.DataCache;
using Glory.Services.Core.DataStore.Providers;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Glory.Provider.DataStore.MongoDB.Core
{
    public class DataStoreMongoDBProvider : DataStoreProvider
    {
        #region Constructor

        private readonly ILogger<DataStoreMongoDBProvider> _logger;
        private readonly IDataCacheManager _dataCacheManager;

        public DataStoreMongoDBProvider(ILogger<DataStoreMongoDBProvider> logger
            , IDataCacheManager dataCacheManager
            )
        {
            _logger = logger;
            _dataCacheManager = dataCacheManager;
        }

        #endregion

        #region Public Properties

        public string ConnectionString { get; set; }
        public string Database { get; set; }

        #endregion

        #region Private Methods

        private IMongoDatabase GetClient()
        {
            var client = new MongoClient(ConnectionString);
            return client?.GetDatabase(Database);
        }
        private IMongoCollection<T> GetCollection<T>()
        {
            var client = GetClient();
            var collectionName = typeof(T).Name;
            var collection = client.GetCollection<T>(collectionName);
            return collection;
        }

        #endregion

        #region Abstract Method Implementation

        public override async Task Insert<T>(T doc)
        {
            _logger.LogTrace("Insert:start");

            var collection = GetCollection<T>();

            await collection.InsertOneAsync(doc);

            _logger.LogTrace("Insert:end");
        }

        public override async Task InsertMany<T>(IEnumerable<T> documents)
        {
            _logger.LogTrace("InsertMany:start");

            var collection = GetCollection<T>();
            await collection.InsertManyAsync(documents);

            _logger.LogTrace("InsertMany:end");
        }

        public override IQueryable<T> Queryable<T>()
        {
            var collection = GetCollection<T>();
            return collection.AsQueryable();
        }

        public override async Task<bool> Remove<T>(Expression<Func<T, bool>> filter)
        {
            _logger.LogTrace("Remove:start");

            var collection = GetCollection<T>();
            var result = await collection.DeleteOneAsync<T>(filter);

            _logger.LogTrace("Remove:end");

            return result.IsAcknowledged;
        }

        public override async Task<bool> RemoveMany<T>(Expression<Func<T, bool>> filter)
        {
            _logger.LogTrace("RemoveMany:start");

            var collection = GetCollection<T>();
            var result = await collection.DeleteManyAsync(filter);

            _logger.LogTrace("RemoveMany:end");

            return result.IsAcknowledged;
        }

        public override async Task<bool> Update<T>(Expression<Func<T, bool>> filter, T doc)
        {

            var collection = GetCollection<T>();
            var result = await collection.ReplaceOneAsync<T>(filter, doc);

            _logger.LogTrace("Update:end");

            return result.IsAcknowledged;
        }

        public override async Task InsertStageData<T>(int stageMinutes, T doc, string createdDateField)
        {
            _logger.LogTrace("InsertStageData:start");

            var collection = GetCollection<T>();
            if (stageMinutes > 0)
            {
                var now = DateTimeOffset.Now;
                string cacheKey = "StageData_PurgeDate_M" + stageMinutes;
                var lastPurgeTime = _dataCacheManager.GetCache<DateTimeOffset?>(cacheKey);
                if (lastPurgeTime == null || now.Subtract(lastPurgeTime.Value).TotalMinutes > stageMinutes)
                {
                    var olderThanDate = now.AddMinutes(0 - stageMinutes);
                    var ltfilter = new FilterDefinitionBuilder<T>().Lt(createdDateField, olderThanDate);
                    var result = await collection.DeleteManyAsync(ltfilter);

                    _logger.LogInformation($"InsertStageData:Purge:Minutes:{stageMinutes}, Count:{result.DeletedCount}");

                    _dataCacheManager.SetCache(cacheKey, now);
                }
            }

            await collection.InsertOneAsync(doc);

            _logger.LogTrace("InsertStageData:start");
        }

        #endregion
    }
}
