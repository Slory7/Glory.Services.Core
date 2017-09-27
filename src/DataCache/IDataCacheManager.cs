using System;
using Glory.Services.Core.DataCache.Parameters;
using Glory.Services.Core.Config;
using System.Collections.Generic;

namespace Glory.Services.Core.DataCache
{
    public interface IDataCacheManager
    {
        void ClearCache();
        void ClearCache(string scope);
        long DecrementValue(string key, Func<long> initialCallBack, int count = 1, int expiredMinutes = 0, ProviderLevel level = ProviderLevel.Normal);
        T DequeueList<T>(string listName, ProviderLevel level = ProviderLevel.Normal);
        void EnqueueList<T>(string listName, T value, ProviderLevel level = ProviderLevel.Normal);
        TObject GetCache<TObject>(string cacheKey, string scope = null, ProviderLevel level = ProviderLevel.Normal);
        TObject GetCachedData<TObject>(CacheItemArgs cacheItemArgs, CacheItemExpiredCallback<TObject> cacheItemExpired);
        T GetCachedDataLocalDistributed<T>(string cacheKey, CacheItemExpiredCallback<T> cacheItemExpiredCallBack, int expireMinutes = 0);
        void RemoveCacheLocalDistributed(string cacheKey);
        T GetItemFromList<T>(string listName, int listIndex, ProviderLevel level = ProviderLevel.Normal);
        long GetListCount(string listName, ProviderLevel level = ProviderLevel.Normal);
        long GetHashCount(string hashId, ProviderLevel level = ProviderLevel.Normal);
        T GetValueFromHash<TKey, T>(string hashId, TKey key, ProviderLevel level = ProviderLevel.Normal);
        long IncrementValue(string key, Func<long> initialCallBack, int count = 1, int expiredMinutes = 0, ProviderLevel level = ProviderLevel.Normal);
        void RemoveCache(string cacheKey);
        void RemoveCache(string cacheKey, ProviderLevel level, string Scope = null);
        bool RemoveEntryFromHash<TKey>(string hashId, TKey key, ProviderLevel level = ProviderLevel.Normal);
        void RemoveFromList<T>(string listName, T value, ProviderLevel level = ProviderLevel.Normal);
        void RemoveList(string listName, ProviderLevel level = ProviderLevel.Normal);
        void SetCache(string cacheKey, object value, string scope = null, DateTimeOffset? absoluteExpiration = null, TimeSpan? slidingExpiration = null, ProviderLevel level = ProviderLevel.Normal, FileCacheDependency dependency = null, RemoveDelegate onRemoveCallback = null);
        void SetEntryInHash<TKey, T>(string hashId, TKey key, T value, ProviderLevel level = ProviderLevel.Normal);
        void SetItemInList<T>(string listName, int listIndex, T value, ProviderLevel level = ProviderLevel.Normal);
        long IncrementValueInHash(string hashId, string key, int count, ProviderLevel level = ProviderLevel.Normal);
        long DecrementValueInHash(string hashId, string key, int count, ProviderLevel level = ProviderLevel.Normal);
        List<T> Sort<T>(string collectionKey, string byField = null, bool fieldIsNumber = true, int skip = 0, int take = -1, bool isAscending = true, ProviderLevel level = ProviderLevel.Normal);
    }
}