using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using Glory.Services.Core.DataCache.Parameters;
using System.Threading;
using System.Collections;
using System.Linq;
using System.Linq.Dynamic.Core;
using Microsoft.Extensions.Primitives;
using Microsoft.Extensions.Logging;
using Glory.Services.Core.Common;

namespace Glory.Services.Core.DataCache.Providers
{
    public class MemoryCacheProvider : CachingProvider
    {
        private const string _defaultScopeName = "$defaultscope";
        private readonly ILogger<MemoryCacheProvider> _logger;
        private readonly IMemoryCache _cache;
        public MemoryCacheProvider(ILogger<MemoryCacheProvider> logger, IMemoryCache cache)
        {
            _logger = logger;
            _cache = cache;
        }

        #region Abstract Method Implementation

        public override T GetItem<T>(string cacheKey, string scope = null)
        {
            string strKey = GetCacheKey(cacheKey, scope);
            T objValue = _cache.Get<T>(strKey);
            return objValue;
        }

        public override void Insert(string cacheKey, object itemToCache, string scope = null, DateTimeOffset? absoluteExpiration = null, TimeSpan? slidingExpiration = null, FileCacheDependency dependency = null,
                                   RemoveDelegate onRemoveCallback = null)
        {
            var options = new MemoryCacheEntryOptions()
            {
                SlidingExpiration = slidingExpiration,
                AbsoluteExpiration = absoluteExpiration
            };
            if (onRemoveCallback != null)
            {
                options.RegisterPostEvictionCallback((key, value, reason, state) =>
                {
                    onRemoveCallback(key, value, (RemoveReason)Enum.Parse(typeof(RemoveReason), reason.ToString()), state);
                });
            }

            if (dependency != null)
                options.AddExpirationToken(dependency.GetChangeToken());

            if (scope != null)
            {
                var ctsScope = UniqueObject.GetUniqueObject<CancellationTokenSource>(GetCacheKeyPrefix(scope));
                options.AddExpirationToken(new CancellationChangeToken(ctsScope.Token));
            }
            var ctsDefault = UniqueObject.GetUniqueObject<CancellationTokenSource>(GetCacheKeyPrefix(_defaultScopeName));
            options.AddExpirationToken(new CancellationChangeToken(ctsDefault.Token));

            string strKey = GetCacheKey(cacheKey, scope);
            _cache.Set(strKey, itemToCache, options);
        }

        public override void Remove(string cacheKey, string scope = null)
        {
            string strKey = GetCacheKey(cacheKey, scope);
            _cache.Remove(strKey);
        }

        public override long GetListCount(string listName)
        {
            string strKey = GetCacheKey(listName);

            var objValue = _cache.Get<ICollection>(strKey);

            if (objValue != null)
                return objValue.Count;

            return 0;
        }

        public override List<T> GetListRange<T>(string listName, long start = 0, long stop = -1)
        {
            string strKey = GetCacheKey(listName);

            var objValue = _cache.Get<List<T>>(strKey);

            var endIndex = stop > 0 ? start + stop : objValue.Count + stop;
            int nstart = (int)start;
            int nEndIndex = (int)endIndex;

            var objList = objValue.AsQueryable().Skip(nstart).Take(nEndIndex).ToList();
            return objList;
        }

        public override T GetItemFromList<T>(string listName, int listIndex)
        {
            string strKey = GetCacheKey(listName);

            var objValue = _cache.Get<List<T>>(strKey);

            if (objValue != null)
                return objValue[listIndex];

            return default(T);
        }

        public override void SetItemInList<T>(string listName, int listIndex, T value)
        {
            string strKey = GetCacheKey(listName);
            var objValue = _cache.Get<List<T>>(strKey);

            if (objValue != null)
            {
                objValue[listIndex] = value;
            }
        }

        public override void EnqueueList<T>(string listName, T value)
        {
            string strKey = GetCacheKey(listName);
            var objValue = _cache.Get<List<T>>(strKey);

            if (objValue == null)
            {
                objValue = SafeCreate<List<T>>(strKey);
            }
            objValue.Add(value);
        }

        public override T DequeueList<T>(string listName)
        {
            string strKey = GetCacheKey(listName);
            var objValue = _cache.Get<List<T>>(strKey);

            if (objValue != null)
            {
                if (objValue.Count > 0)
                {
                    var tValue = objValue[0];
                    objValue.RemoveAt(0);
                    return tValue;
                }
            }
            return default(T);
        }

        public override void RemoveFromList<T>(string listName, T value)
        {
            string strKey = GetCacheKey(listName);
            var objValue = _cache.Get<List<T>>(strKey);

            if (objValue != null)
            {
                if (objValue.Count > 0)
                {
                    objValue.Remove(value);
                }
            }
        }

        public override void RemoveList(string listName)
        {
            string strKey = GetCacheKey(listName);
            this.Remove(strKey);
        }

        public override long GetHashCount(string hashId)
        {
            string strKey = GetCacheKey(hashId);

            var objValue = _cache.Get<IDictionary>(strKey);

            if (objValue != null)
                return objValue.Count;

            return 0;
        }

        public override void SetEntryInHash<TKey, T>(string hashId, TKey key, T value)
        {
            string strKey = GetCacheKey(hashId);
            var objValue = _cache.Get<Dictionary<TKey, T>>(strKey);

            if (objValue == null)
            {
                objValue = SafeCreate<Dictionary<TKey, T>>(strKey);
            }
            objValue.Add(key, value);
        }

        public override bool RemoveEntryFromHash<TKey>(string hashId, TKey key)
        {
            string strKey = GetCacheKey(hashId);
            var objValue = _cache.Get<Dictionary<TKey, object>>(strKey);

            if (objValue != null)
            {
                return objValue.Remove(key);
            }
            return false;
        }

        public override T GetValueFromHash<TKey, T>(string hashId, TKey key)
        {
            string strKey = GetCacheKey(hashId);
            var objValue = _cache.Get<Dictionary<TKey, T>>(strKey);

            if (objValue != null)
            {
                if (objValue.ContainsKey(key))
                    return objValue[key];
            }
            return default(T);
        }

        public override long IncrementValue(string key, int count, int expiredMinutes, Func<long> initialCallBack)
        {
            long val = 0;
            string strKey = GetCacheKey(key);
            object objValue = _cache.Get(strKey);

            if (objValue == null)
            {
                objValue = initialCallBack();
                _logger.LogInformation("IncrementValue initialcallback called:key:" + key);
            }
            val = (long)objValue + count;

            var absoluteExpiration = expiredMinutes > 0 ? DateTimeOffset.Now.AddMinutes(expiredMinutes) : (DateTimeOffset?)null;
            this.Insert(key, val, absoluteExpiration: absoluteExpiration);

            return val;
        }

        public override long DecrementValue(string key, int count, int expiredMinutes, Func<long> initialCallBack)
        {
            long val = 0;
            string strKey = GetCacheKey(key);
            object objValue = _cache.Get(strKey);

            if (objValue == null)
            {
                objValue = initialCallBack();
                _logger.LogInformation("DecrementValue initialcallback called:key:" + key);
            }
            val = (long)objValue - count;

            var absoluteExpiration = expiredMinutes > 0 ? DateTimeOffset.Now.AddMinutes(expiredMinutes) : (DateTimeOffset?)null;
            this.Insert(key, val, absoluteExpiration: absoluteExpiration);

            return val;
        }

        public override long IncrementValueInHash(string hashId, string key, int count)
        {
            string strKey = GetCacheKey(hashId);
            var objValue = _cache.Get<Dictionary<string, long>>(strKey);

            if (objValue == null)
            {
                objValue = SafeCreate<Dictionary<string, long>>(strKey);
            }
            long retVal;
            if (objValue.ContainsKey(key))
            {
                retVal = objValue[key] + count;
                objValue[key] = retVal;
            }
            else
            {
                retVal = count;
                objValue.Add(key, count);
            }
            return retVal;
        }

        public override long DecrementValueInHash(string hashId, string key, int count)
        {
            return IncrementValueInHash(hashId, key, 0 - count);
        }

        public override List<T> Sort<T>(string collectionKey, string byField, bool fieldIsNumber, int skip, int take, bool isAscending)
        {
            string strKey = GetCacheKey(collectionKey);
            var objValue = _cache.Get(strKey);
            if (objValue != null)
            {
                var objCollection = objValue as ICollection<T>;
                if (objCollection == null)
                {
                    var objDic = objValue as IDictionary<object, T>;
                    if (objDic != null)
                    {
                        objCollection = objDic.Values;
                    }
                }
                var query = objCollection.AsQueryable();
                if (skip > 0)
                    query = query.Skip(skip);
                if (take > 0)
                    query = query.Take(take);
                IOrderedQueryable<T> orderedQueryable = null;
                if (byField == null)
                    orderedQueryable = isAscending ? query.OrderBy(c => c) : query.OrderByDescending(c => c);
                else
                    orderedQueryable = query.OrderBy(byField + (isAscending ? " ASC" : " DESC"));
                var resultList = orderedQueryable.ToList();
                return resultList;
            }
            return new List<T>(0);
        }

        public override bool ExpireItem(string key, DateTime? expireTime)
        {
            string strKey = GetCacheKey(key);

            object objValue = _cache.Get(strKey);

            this.Insert(key, objValue, absoluteExpiration: expireTime);

            return true;
        }

        public override void Clear(string scope = null)
        {
            var strScope = scope ?? _defaultScopeName;
            var ctsScope = UniqueObject.GetUniqueObject<CancellationTokenSource>(GetCacheKeyPrefix(strScope));
            ctsScope.Cancel();
        }

        public override bool IsDistributedCache()
        {
            return false;
        }

        #endregion

        #region Private Methods

        private T SafeCreate<T>(string itemKey) where T : new()
        {
            lock (UniqueObject.GetUniqueObject<object>(itemKey))
            {
                object objValue = _cache.Get(itemKey);
                if (objValue == null)
                {
                    var objItem = new T();
                    this.Insert(itemKey, objItem);
                    return objItem;
                }
                return (T)objValue;
            }
        }

        #endregion
    }
}
