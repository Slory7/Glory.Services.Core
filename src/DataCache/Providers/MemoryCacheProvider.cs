using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using Glory.Services.Core.DataCache.Parameters;
using System.Threading;
using Microsoft.Extensions.Primitives;
using Microsoft.Extensions.Logging;

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
                var ctsScope = DataCacheManager.GetUniqueLockObject<CancellationTokenSource>(GetCacheKeyPrefix(scope));
                options.AddExpirationToken(new CancellationChangeToken(ctsScope.Token));
            }
            var ctsDefault = DataCacheManager.GetUniqueLockObject<CancellationTokenSource>(GetCacheKeyPrefix(_defaultScopeName));
            options.AddExpirationToken(new CancellationChangeToken(ctsDefault.Token));

            string strKey = GetCacheKey(cacheKey, scope);
            _cache.Set(strKey, itemToCache, options);
        }

        public override void Remove(string cacheKey, string scope = null)
        {
            string strKey = GetCacheKey(cacheKey, scope);
            _cache.Remove(strKey);
        }

        public override long GetListCount<T>(string listName)
        {
            string strKey = GetCacheKey(listName);
            object objValue = _cache.Get(strKey);
            if (objValue != null)
                return ((List<T>)objValue).Count;
            return 0;
        }

        public override T GetItemFromList<T>(string listName, int listIndex)
        {
            string strKey = GetCacheKey(listName);
            object objValue = _cache.Get(strKey);
            if (objValue != null)
                return ((List<T>)objValue)[listIndex];
            return default(T);
        }

        public override void SetItemInList<T>(string listName, int listIndex, T value)
        {
            string strKey = GetCacheKey(listName);
            object objValue = _cache.Get(strKey);

            if (objValue != null)
            {
                ((List<T>)objValue)[listIndex] = value;
            }
        }

        public override void EnqueueList<T>(string listName, T value)
        {
            string strKey = GetCacheKey(listName);
            object objValue = _cache.Get(strKey);

            List<T> objList = null;
            if (objValue == null)
            {
                objList = new List<T>();
                this.Insert(listName, objList);
            }
            else
            {
                objList = (List<T>)objValue;
            }
            objList.Add(value);
        }

        public override T DequeueList<T>(string listName)
        {
            string strKey = GetCacheKey(listName);
            object objValue = _cache.Get(strKey);

            if (objValue != null)
            {
                var objList = ((List<T>)objValue);
                if (objList.Count > 0)
                {
                    var tValue = objList[0];
                    objList.RemoveAt(0);
                    return tValue;
                }
            }
            return default(T);
        }

        public override void RemoveFromList<T>(string listName, T value)
        {
            string strKey = GetCacheKey(listName);
            object objValue = _cache.Get(strKey);

            if (objValue != null)
            {
                var objList = ((List<T>)objValue);
                if (objList.Count > 0)
                {
                    objList.Remove(value);
                }
            }
        }

        public override void RemoveList(string listName)
        {
            string strKey = GetCacheKey(listName);
            this.Remove(strKey);
        }

        public override void SetEntryInHash<TKey, T>(string hashId, TKey key, T value)
        {
            string strKey = GetCacheKey(hashId);
            object objValue = _cache.Get(strKey);

            Dictionary<TKey, T> objDic = null;
            if (objValue == null)
            {
                objDic = new Dictionary<TKey, T>();
                this.Insert(hashId, objDic);
            }
            else
            {
                objDic = (Dictionary<TKey, T>)objValue;
            }
            objDic.Add(key, value);
        }

        public override bool RemoveEntryFromHash<TKey>(string hashId, TKey key)
        {
            string strKey = GetCacheKey(hashId);
            object objValue = _cache.Get(strKey);

            if (objValue != null)
            {
                var objDic = (Dictionary<TKey, object>)objValue;
                return objDic.Remove(key);
            }

            return false;
        }

        public override T GetValueFromHash<TKey, T>(string hashId, TKey key)
        {
            string strKey = GetCacheKey(hashId);
            object objValue = _cache.Get(strKey);

            if (objValue != null)
            {
                var objDic = (Dictionary<TKey, T>)objValue;
                if (objDic.ContainsKey(key))
                    return objDic[key];
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

        public override void Clear(string scope = null)
        {
            var strScope = scope ?? _defaultScopeName;
            var ctsScope = DataCacheManager.GetUniqueLockObject<CancellationTokenSource>(GetCacheKeyPrefix(strScope));
            ctsScope.Cancel();
        }

        public override bool IsDistributedCache()
        {
            return false;
        }

        #endregion
    }
}
