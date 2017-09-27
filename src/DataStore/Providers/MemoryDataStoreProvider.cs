using Glory.Services.Core.Config;
using Glory.Services.Core.DataCache;
using Glory.Services.Core.DataCache.Parameters;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Glory.Services.Core.Common;

namespace Glory.Services.Core.DataStore.Providers
{
    public class MemoryDataStoreProvider : DataStoreProvider
    {
        private readonly IDataCacheManager _cacheManager;
        private readonly ILogger<MemoryDataStoreProvider> _logger;
        public MemoryDataStoreProvider(IDataCacheManager cacheManager
            , ILogger<MemoryDataStoreProvider> logger
            )
        {
            _cacheManager = cacheManager;
            _logger = logger;
        }

        #region Abstract Method Implementation

        public override async Task Insert<T>(T doc)
        {
            string cacheKey = typeof(T).Name;
            await Task.Run(() =>
            {
                var list = _cacheManager.GetCachedData(new CacheItemArgs(cacheKey), (args) =>
                  {
                      return new List<T>();
                  });
                list.Add(doc);
            });
        }

        public override async Task InsertMany<T>(IEnumerable<T> documents)
        {
            string cacheKey = typeof(T).Name;
            await Task.Run(() =>
            {
                var list = _cacheManager.GetCachedData(new CacheItemArgs(cacheKey), (args) =>
                  {
                      return new List<T>();
                  });
                list.AddRange(documents);
            });
        }

        public override IQueryable<T> Queryable<T>()
        {
            string cacheKey = typeof(T).Name;

            var list = _cacheManager.GetCachedData(new CacheItemArgs(cacheKey), (args) =>
              {
                  return new List<T>();
              });
            return list.AsQueryable();
        }

        public override async Task<bool> Remove<T>(Expression<Func<T, bool>> filter)
        {
            string cacheKey = typeof(T).Name;
            await Task.Run(() =>
            {
                var list = _cacheManager.GetCachedData(new CacheItemArgs(cacheKey), (args) =>
                  {
                      return new List<T>();
                  });
                var f = filter.Compile();
                var item = list.Where(f).SingleOrDefault();
                if (item != null)
                    list.Remove(item);
            });
            return true;
        }

        public override async Task<bool> RemoveMany<T>(Expression<Func<T, bool>> filter)
        {
            string cacheKey = typeof(T).Name;
            await Task.Run(() =>
            {
                var list = _cacheManager.GetCachedData(new CacheItemArgs(cacheKey), (args) =>
                  {
                      return new List<T>();
                  });
                var f = filter.Compile();
                list.Where(f).ToList().ForEach(t => list.Remove(t));
            });
            return true;
        }

        public override async Task<bool> Update<T>(Expression<Func<T, bool>> filter, T doc)
        {
            string cacheKey = typeof(T).Name;
            await Task.Run(() =>
            {
                var list = _cacheManager.GetCachedData(new CacheItemArgs(cacheKey), (args) =>
                  {
                      return new List<T>();
                  });
                var f = filter.Compile();
                var item = list.Where(f).SingleOrDefault();
                if (item != null)
                    list.Remove(item);
                list.Add(doc);
            });
            return true;
        }

        public override async Task<T> IncrementField<T>(Expression<Func<T, bool>> filter, string field, int amount)
        {
            string cacheKey = typeof(T).Name;
            T item = default(T);
            await Task.Run(() =>
            {
                var list = _cacheManager.GetCachedData(new CacheItemArgs(cacheKey), (args) =>
                  {
                      return new List<T>();
                  });
                var f = filter.Compile();
                lock (UniqueObject.GetUniqueObject<object>(cacheKey))
                {
                    item = list.Where(f).SingleOrDefault();
                    if (item != null)
                    {
                        var prop = item.GetType().GetProperty(field);
                        if (prop != null)
                        {
                            var propType = prop.PropertyType;

                            var val = (long)prop.GetValue(item) + amount;

                            var objVal = Convert.ChangeType(val, propType);
                            prop.SetValue(item, objVal);
                        }
                    }
                }
            });
            return item;
        }

        public override async Task InsertStageData<T>(int stageMinutes, T doc, string createdDateField)
        {
            string cacheKey = typeof(T).Name + "_" + stageMinutes;
            var list = _cacheManager.GetCachedData(new CacheItemArgs(cacheKey), (args) =>
            {
                return new List<T>();
            });

            if (stageMinutes > 0 && list.Count > 0)
            {
                var now = DateTimeOffset.Now;
                string cachePugeKey = "StageData_PurgeDate_M" + stageMinutes;
                var lastPurgeTime = _cacheManager.GetCache<DateTimeOffset?>(cachePugeKey);
                if (lastPurgeTime == null || now.Subtract(lastPurgeTime.Value).TotalMinutes > stageMinutes)
                {
                    var olderThanDate = now.AddMinutes(0 - stageMinutes);
                    await Task.Run(() =>
                    {
                        var toRemoveList = list.AsQueryable().Where($"{createdDateField}<DateTimeOffset.Parse(\"{olderThanDate}\")").ToList();
                        toRemoveList.ForEach(t => list.Remove(t));

                        _logger.LogInformation($"InsertStageData:Purge:Minutes:{stageMinutes}, Count:{toRemoveList.Count}");
                    });
                    _cacheManager.SetCache(cachePugeKey, now);
                }
            }
            list.Add(doc);
        }

        #endregion
    }
}
