using StackExchange.Redis;
using System;
using Glory.Services.Core;
using Glory.Services.Core.DataCache.Providers;
using Glory.Services.Core.DataCache.Parameters;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace Glory.Provider.DataCache.Redis.Core
{
    public class RedisCacheProvider : CachingProvider
    {
        #region Constructor

        private readonly ILogger<RedisCacheProvider> _logger;
        public RedisCacheProvider(ILogger<RedisCacheProvider> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #endregion

        #region Properties

        public string Servers { get; set; }
        public string Password { get; set; }
        public string ServerType { get; set; }
        public string MasterDB { get; set; }
        public int DBNumber { get; set; }
        public bool Ssl { get; set; }
        public bool AllowAdmin { get; set; }
        public int ConnectRetry { get; set; }
        public int ConnectTimeout { get; set; }
        public string ClientName { get; set; }
        public bool AbortOnConnectFail { get; set; }

        public string ConfigurationString { get; set; }

        #endregion

        #region Abstract Method Implementation             

        public override T GetItem<T>(string cacheKey, string scope = null)
        {
            var key = GetCacheKey(cacheKey, scope);
            using (var redisClient = this.GetRedisClient())
            {
                T objValue = redisClient.Get<T>(key);
                return objValue;
            }
        }

        public override void Remove(string cacheKey, string scope = null)
        {
            var key = GetCacheKey(cacheKey, scope);
            using (var redisClient = this.GetRedisClient())
            {
                redisClient.Remove(key);
            }
        }

        public override void Insert(string cacheKey, object itemToCache, string scope = null, DateTimeOffset? absoluteExpiration = null, TimeSpan? slidingExpiration = null, FileCacheDependency dependency = null,
                                   RemoveDelegate onRemoveCallback = null)
        {
            if (dependency != null)
            {
                throw new NotSupportedException("RedisCacheProvider does not support file dependency.");
            }
            var key = GetCacheKey(cacheKey, scope);
            using (var redisClient = this.GetRedisClient())
            {
                if (absoluteExpiration != null)
                    redisClient.Add(key, itemToCache, absoluteExpiration.Value);
                else if (slidingExpiration != null)
                    redisClient.Add(key, itemToCache, slidingExpiration.Value);
                else
                    redisClient.Add(key, itemToCache);
            }
        }

        public override void Clear(string scope = null)
        {
            var prefix = GetCacheKeyPrefix(scope);
            ClearCacheInternal(prefix);
        }

        public override long GetListCount(string listName)
        {
            var key = GetCacheKey(listName);
            using (var redisClient = this.GetRedisClient())
            {
                long lCount = redisClient.GetListCount(key);
                return lCount;
            }
        }
        public override List<T> GetListRange<T>(string listName, long start = 0, long stop = -1)
        {
            var key = GetCacheKey(listName);
            using (var redisClient = this.GetRedisClient())
            {
                var objValues = redisClient.GetListRange<T>(listName, start, stop);
                return objValues;
            }
        }
        public override T GetItemFromList<T>(string listName, int listIndex)
        {
            var key = GetCacheKey(listName);
            using (var redisClient = this.GetRedisClient())
            {
                var objValue = redisClient.GetItemFromList<T>(key, listIndex);
                return objValue;
            }
        }

        public override void SetItemInList<T>(string listName, int listIndex, T value)
        {
            var key = GetCacheKey(listName);
            using (var redisClient = this.GetRedisClient())
            {
                redisClient.SetItemInList(key, listIndex, value);
            }
        }

        public override void EnqueueList<T>(string listName, T value)
        {
            var key = GetCacheKey(listName);
            using (var redisClient = this.GetRedisClient())
            {
                redisClient.EnqueueList(key, value);
            }
        }

        public override T DequeueList<T>(string listName)
        {
            var key = GetCacheKey(listName);
            using (var redisClient = this.GetRedisClient())
            {
                var objValue = redisClient.DequeueList<T>(key);
                return objValue;
            }
        }

        public override void RemoveFromList<T>(string listName, T value)
        {
            var key = GetCacheKey(listName);
            using (var redisClient = this.GetRedisClient())
            {
                redisClient.RemoveFromList(key, value);
            }
        }

        public override void RemoveList(string listName)
        {
            string key = GetCacheKey(listName);
            this.Remove(key);
        }

        public override long GetHashCount(string hashId)
        {
            var key = GetCacheKey(hashId);
            using (var redisClient = this.GetRedisClient())
            {
                long lCount = redisClient.GetHashCount(key);
                return lCount;
            }
        }

        public override T GetValueFromHash<TKey, T>(string hashId, TKey key)
        {
            var strKey = GetCacheKey(hashId);
            using (var redisClient = this.GetRedisClient())
            {
                var valueFromHash = redisClient.GetValueFromHash<TKey, T>(strKey, key);
                return valueFromHash;
            }
        }

        public override bool RemoveEntryFromHash<TKey>(string hashId, TKey key)
        {
            var strKey = GetCacheKey(hashId);
            using (var redisClient = this.GetRedisClient())
            {
                bool flag = redisClient.RemoveEntryFromHash(strKey, key);
                return flag;
            }
        }

        public override void SetEntryInHash<TKey, T>(string hashId, TKey key, T value)
        {
            string strKey = GetCacheKey(hashId);
            using (var redisClient = this.GetRedisClient())
            {
                redisClient.SetEntryInHash<TKey, T>(strKey, key, value);
            }
        }

        public override long IncrementValue(string key, int count, int expiredMinutes, Func<long> initialCallBack)
        {
            string strKey = GetCacheKey(key);
            long retVal;
            using (var redisClient = this.GetRedisClient())
            {
                var objValue = redisClient.Get<object>(strKey);
                if (objValue == null)
                {
                    using (redisClient.AcquireLock(strKey + "$lock", TimeSpan.FromSeconds(5)))
                    {
                        objValue = redisClient.Get<object>(strKey);
                        if (objValue == null)
                        {
                            var val = initialCallBack();

                            _logger.LogInformation("IncrementValue initialcallback called:key:" + key);

                            if (expiredMinutes > 0)
                                redisClient.Add(strKey, val, DateTime.Now.AddMinutes(expiredMinutes));
                            else
                                redisClient.Add(strKey, val);
                        }
                    }
                }
                retVal = redisClient.IncrementValueBy(strKey, count);
            }
            return retVal;
        }

        public override long DecrementValue(string key, int count, int expiredMinutes, Func<long> initialCallBack)
        {
            string strKey = GetCacheKey(key);
            long retVal;
            using (var redisClient = this.GetRedisClient())
            {
                var objValue = redisClient.Get<object>(strKey);
                if (objValue == null)
                {
                    using (redisClient.AcquireLock(strKey + "$lock", TimeSpan.FromSeconds(5)))
                    {
                        objValue = redisClient.Get<object>(strKey);
                        if (objValue == null)
                        {
                            var val = initialCallBack();

                            _logger.LogInformation("DecrementValue initialcallback called:key:" + key);

                            if (expiredMinutes > 0)
                                redisClient.Add(strKey, val, DateTime.Now.AddMinutes(expiredMinutes));
                            else
                                redisClient.Add(strKey, val);
                        }
                    }
                }
                retVal = redisClient.DecrementValueBy(strKey, count);
            }
            return retVal;
        }

        public override long IncrementValueInHash(string hashId, string key, int count)
        {
            string strKey = GetCacheKey(key);
            using (var redisClient = this.GetRedisClient())
            {
                return redisClient.IncrementValueInHash(strKey, key, count);
            }
        }

        public override long DecrementValueInHash(string hashId, string key, int count)
        {
            string strKey = GetCacheKey(key);
            using (var redisClient = this.GetRedisClient())
            {
                return redisClient.DecrementValueInHash(strKey, key, count);
            }
        }

        public override List<T> Sort<T>(string collectionKey, string byField, bool fieldIsNumber, int skip, int take, bool isAscending)
        {
            string strKey = GetCacheKey(collectionKey);
            using (var redisClient = this.GetRedisClient())
            {
                return redisClient.Sort<T>(strKey, byField, fieldIsNumber, skip, take, isAscending);
            }
        }

        public override bool ExpireItem(string key, DateTime? expireTime)
        {
            string strKey = GetCacheKey(key);
            using (var redisClient = this.GetRedisClient())
            {
                return redisClient.ExpireItem(key, expireTime);
            }
        }

        public override bool IsDistributedCache()
        {
            return true;
        }

        #endregion

        #region Private Methods

        private RedisClient GetRedisClient()
        {
            ConfigurationOptions options = null;
            if (String.IsNullOrEmpty(ConfigurationString))
            {
				var cmdMap = CommandMap.Default;
				switch (ServerType?.ToLower())
				{
					case "sentinel":
						cmdMap = CommandMap.Sentinel;
						break;
					case "ssdb":
						cmdMap = CommandMap.SSDB;
						break;
					case "twemproxy":
						cmdMap = CommandMap.Twemproxy;
						break;
				}

                options = new ConfigurationOptions
                {
                    CommandMap = cmdMap,
                    DefaultDatabase = this.DBNumber,
                    Password = this.Password,
                    ServiceName = this.MasterDB,
                    ClientName = this.ClientName,
                    Ssl = this.Ssl,
                    AllowAdmin = this.AllowAdmin,
                    ConnectTimeout = this.ConnectTimeout,
                    ConnectRetry = this.ConnectRetry,
                    AbortOnConnectFail = this.AbortOnConnectFail
                };

                foreach (string server in this.Servers.Split(','))
                {
                    options.EndPoints.Add(server);
                }
            }
            else
            {
                options = ConfigurationOptions.Parse(ConfigurationString);
            }

            var redisClient = new RedisClient(options);
            return redisClient;
        }

        private void ClearCacheInternal(string prefix)
        {
            using (var redisClient = this.GetRedisClient())
            {
                var matchKeys = redisClient.SearchKeys(prefix + "*");
                redisClient.RemoveAll(matchKeys);
            }
        }

        #endregion

    }
}
