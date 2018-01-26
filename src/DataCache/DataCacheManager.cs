
#region Usings

using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.Logging;
using Glory.Services.Core.DataCache.Parameters;
using Glory.Services.Core.DataCache.Providers;
using Glory.Services.Core.Config;
using Glory.Services.Core.Common;

#endregion

namespace Glory.Services.Core.DataCache
{
    public class DataCacheManager : IDataCacheManager
    {
        private readonly ILogger _logger;
        public DataCacheManager(ILogger<DataCacheManager> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #region Public Methods

        public TObject GetCachedData<TObject>(CacheItemArgs cacheItemArgs, CacheItemExpiredCallback<TObject> cacheItemExpired)
        {
            var objObject = GetCachedDataInternal<TObject>(cacheItemArgs, cacheItemExpired);

            // return the object
            if (objObject == null)
            {
                return default(TObject);
            }
            return objObject;
        }

        public TObject GetCache<TObject>(string CacheKey, string scope = null, ProviderLevel level = ProviderLevel.Normal)
        {
            TObject objObject = CachingProvider.Instance(level).GetItem<TObject>(CacheKey, scope);
            return objObject;
        }

        public void RemoveCache(string CacheKey)
        {
            CachingProvider.RemoveCache(CacheKey, null);
        }

        public void RemoveCache(string CacheKey, ProviderLevel level, string Scope = null)
        {
            CachingProvider.Instance(level).Remove(CacheKey, Scope);
        }

        public void SetCache(string CacheKey, object objObject, string Scope = null, DateTimeOffset? AbsoluteExpiration = null, TimeSpan? SlidingExpiration = null, ProviderLevel level = ProviderLevel.Normal,
                                     FileCacheDependency objDependency = null, RemoveDelegate OnRemoveCallback = null)
        {
            if (objObject != null)
            {
                //if no OnRemoveCallback value is specified, use the default method
                if (OnRemoveCallback == null)
                {
                    OnRemoveCallback = ItemRemovedCallback;
                }
                CachingProvider.Instance(level).Insert(CacheKey, objObject, Scope, AbsoluteExpiration, SlidingExpiration, objDependency, OnRemoveCallback);
            }
        }

        public T GetCachedDataLocalDistributed<T>(string cacheKey, CacheItemExpiredCallback<T> cacheItemExpiredCallBack, int expireMinutes = 0)
        {
            string strCacheLocalKey = cacheKey + "_$local";
            string strCacheRemoteKey = cacheKey + "_$remote";
            var objRemoteVersion = GetCachedData<string>(new CacheItemArgs(strCacheRemoteKey, 0, expireMinutes), (args) =>
           {
               return Guid.NewGuid().ToString();
           });
            var objLocalVersion = GetCache<string>(strCacheLocalKey, level: ProviderLevel.High);
            if (objLocalVersion != objRemoteVersion)
            {
                RemoveCache(cacheKey, ProviderLevel.High);
            }
            var cachedData = GetCachedData<T>(new CacheItemArgs(cacheKey, 0, expireMinutes, ProviderLevel.High), (args) =>
            {
                DateTimeOffset? expireTime = expireMinutes == 0 ? (DateTimeOffset?)null : DateTimeOffset.Now.AddMinutes(expireMinutes);
                SetCache(strCacheLocalKey, objRemoteVersion, AbsoluteExpiration: expireTime, level: ProviderLevel.High);
                return cacheItemExpiredCallBack(args);
            });
            return cachedData;
        }

        public void RemoveCacheLocalDistributed(string cacheKey)
        {
            string strCacheRemoteKey = cacheKey + "_$remote";
            RemoveCache(strCacheRemoteKey, ProviderLevel.Normal);
            RemoveCache(cacheKey, ProviderLevel.High);
        }

        public long GetListCount(string listName, ProviderLevel level)
        {
            var count = CachingProvider.Instance(level).GetListCount(listName);
            return count;
        }

        public List<T> GetListRange<T>(string listName, ProviderLevel level, long start = 0, long stop = -1)
        {
            var items = CachingProvider.Instance(level).GetListRange<T>(listName, start, stop);
            return items;
        }

        public T GetItemFromList<T>(string listName, int listIndex, ProviderLevel level)
        {
            var objValue = CachingProvider.Instance(level).GetItemFromList<T>(listName, listIndex);
            return objValue;
        }

        public void SetItemInList<T>(string listName, int listIndex, T value, ProviderLevel level)
        {
            CachingProvider.Instance(level).SetItemInList<T>(listName, listIndex, value);
        }

        public void EnqueueList<T>(string listName, T value, ProviderLevel level)
        {
            CachingProvider.Instance(level).EnqueueList<T>(listName, value);
        }

        public T DequeueList<T>(string listName, ProviderLevel level)
        {
            var objValue = CachingProvider.Instance(level).DequeueList<T>(listName);
            return objValue;
        }

        public void RemoveFromList<T>(string listName, T value, ProviderLevel level)
        {
            CachingProvider.Instance(level).RemoveFromList<T>(listName, value);
        }

        public void RemoveList(string listName, ProviderLevel level)
        {
            CachingProvider.Instance(level).RemoveList(listName);
        }

        public long GetHashCount(string hashId, ProviderLevel level)
        {
            var count = CachingProvider.Instance(level).GetHashCount(hashId);
            return count;
        }

        public void SetEntryInHash<TKey, T>(string hashId, TKey key, T value, ProviderLevel level)
        {
            CachingProvider.Instance(level).SetEntryInHash<TKey, T>(hashId, key, value);
        }

        public T GetValueFromHash<TKey, T>(string hashId, TKey key, ProviderLevel level)
        {
            return CachingProvider.Instance(level).GetValueFromHash<TKey, T>(hashId, key);
        }

        public bool RemoveEntryFromHash<TKey>(string hashId, TKey key, ProviderLevel level)
        {
            return CachingProvider.Instance(level).RemoveEntryFromHash<TKey>(hashId, key);
        }

        public long IncrementValue(string key, Func<long> initialCallBack, int count = 1, int expiredMinutes = 0, ProviderLevel level = ProviderLevel.Normal)
        {
            var provider = CachingProvider.Instance(level);
            if (!provider.IsDistributedCache())
            {
                object @lock = UniqueObject.GetUniqueObject<object>(key);
                lock (@lock)
                {
                    var retVal = provider.IncrementValue(key, count, expiredMinutes, initialCallBack);
                    UniqueObject.RemoveUniqueObject(key);
                    return retVal;
                }
            }
            return provider.IncrementValue(key, count, expiredMinutes, initialCallBack);
        }

        public long DecrementValue(string key, Func<long> initialCallBack, int count = 1, int expiredMinutes = 0, ProviderLevel level = ProviderLevel.Normal)
        {
            var provider = CachingProvider.Instance(level);
            if (!provider.IsDistributedCache())
            {
                object @lock = UniqueObject.GetUniqueObject<object>(key);
                lock (@lock)
                {
                    var retVal = provider.DecrementValue(key, count, expiredMinutes, initialCallBack);
                    UniqueObject.RemoveUniqueObject(key);
                    return retVal;
                }
            }
            return provider.DecrementValue(key, count, expiredMinutes, initialCallBack);
        }

        public long IncrementValueInHash(string hashId, string key, int count, ProviderLevel level)
        {
            var provider = CachingProvider.Instance(level);
            if (!provider.IsDistributedCache())
            {
                object @lock = UniqueObject.GetUniqueObject<object>(key);
                lock (@lock)
                {
                    var retVal = provider.IncrementValueInHash(hashId, key, count);
                    UniqueObject.RemoveUniqueObject(key);
                    return retVal;
                }
            }
            return provider.IncrementValueInHash(hashId, key, count);
        }

        public long DecrementValueInHash(string hashId, string key, int count, ProviderLevel level)
        {
            var provider = CachingProvider.Instance(level);
            if (!provider.IsDistributedCache())
            {
                object @lock = UniqueObject.GetUniqueObject<object>(key);
                lock (@lock)
                {
                    var retVal = provider.DecrementValueInHash(hashId, key, count);
                    UniqueObject.RemoveUniqueObject(key);
                    return retVal;
                }
            }
            return provider.DecrementValueInHash(hashId, key, count);
        }

        public List<T> Sort<T>(string collectionKey, string byField, bool fieldIsNumber, int skip, int take, bool isAscending, ProviderLevel level)
        {
            return CachingProvider.Instance(level).Sort<T>(collectionKey, byField, fieldIsNumber, skip, take, isAscending);
        }

        public bool ExpireItem(string cacheKey, DateTime? expireTime, ProviderLevel level)
        {
            return CachingProvider.Instance(level).ExpireItem(cacheKey, expireTime);
        }

        public void ClearCache()
        {
            CachingProvider.ClearCache();
            //log the cache clear event
            _logger.LogInformation("CACHE_REFRESH");
        }

        public void ClearCache(string scope)
        {
            CachingProvider.ClearCache(scope);
            _logger.LogInformation("CACHE_CLEAR:Scope:" + scope);
        }

        #endregion

        #region Private Methods

        private TObject GetCachedDataInternal<TObject>(CacheItemArgs cacheItemArgs, CacheItemExpiredCallback<TObject> cacheItemExpired)
        {
            TObject objObject = GetCache<TObject>(cacheItemArgs.CacheKey, cacheItemArgs.Scope, cacheItemArgs.Level);

            // if item is not cached
            if (EqualityComparer<TObject>.Default.Equals(objObject, default(TObject)))
            {
                //Get Unique Lock for cacheKey
                object @lock = UniqueObject.GetUniqueObject<object>(cacheItemArgs.CacheKey);

                // prevent other threads from entering this block while we regenerate the cache
                lock (@lock)
                {
                    // try to retrieve object from the cache again (in case another thread loaded the object since we first checked)
                    objObject = GetCache<TObject>(cacheItemArgs.CacheKey, cacheItemArgs.Scope, cacheItemArgs.Level);

                    // if object was still not retrieved

                    if (EqualityComparer<TObject>.Default.Equals(objObject, default(TObject)))
                    {
                        // get object from data source using delegate
                        //try
                        //{
                        objObject = cacheItemExpired(cacheItemArgs);
                        _logger.LogInformation("GetCacheItemFromExpiredCallback:" + cacheItemArgs.CacheKey);
                        //}
                        //catch (Exception ex)
                        //{
                        //     objObject = default(TObject);
                        //     _logger.LogError(ex, "CacheItemExpiredCallbackError");
                        // }

                        // if we retrieved a valid object and we are using caching
                        if (objObject != null)
                        {
                            // save the object in the cache
                            SetCache(cacheItemArgs.CacheKey,
                                     objObject,
                                     cacheItemArgs.Scope,
                                     cacheItemArgs.ExpireTimeOutMinutes > 0 ? DateTimeOffset.Now.AddMinutes(cacheItemArgs.ExpireTimeOutMinutes) : (DateTimeOffset?)null,
                                     cacheItemArgs.CacheTimeOutMinutes > 0 ? TimeSpan.FromMinutes(cacheItemArgs.CacheTimeOutMinutes) : (TimeSpan?)null,
                                     cacheItemArgs.Level,
                                     cacheItemArgs.CacheDependency,
                                     cacheItemArgs.CacheCallback);

                            // check if the item was actually saved in the cache

                            if (GetCache<TObject>(cacheItemArgs.CacheKey, cacheItemArgs.Scope, cacheItemArgs.Level) == null)
                            {
                                // log the event if the item was not saved in the cache ( likely because we are out of memory )
                                _logger.LogError("CACHE_OVERFLOW:" + cacheItemArgs.CacheKey + ":Overflow - Item Not Cached");
                            }
                        }

                        //This thread won so remove unique Lock from collection
                        UniqueObject.RemoveUniqueObject(cacheItemArgs.CacheKey);
                    }
                }
            }

            return objObject;
        }

        private void ItemRemovedCallback(object key, object value, RemoveReason removedReason, object state)
        {
            //if the item was removed from the cache, log the key and reason to the event log
            try
            {
                _logger.LogInformation("ItemRemovedCallback:key:" + key + ", reason:" + removedReason.ToString());
            }
            catch (Exception exc)
            {
                //Swallow exception            
                _logger.LogError(exc, "ItemRemovedCallbackError");
            }
        }

        #endregion

    }
}