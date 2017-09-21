
#region Usings

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Glory.Services.Core.DataCache.Parameters;
using Glory.Services.Core.Config;
using Glory.Services.Core.DataCache.Config;

#endregion

namespace Glory.Services.Core.DataCache.Providers
{
    public abstract class CachingProvider
    {
        #region Private Members

        private const string CachePrefix = "GLY_";

        private static CachingProvider defaultCachingProvider = null;
        private static Dictionary<ProviderLevel, CachingProvider> providerInstances =
            new Dictionary<ProviderLevel, CachingProvider>();

        #endregion

        #region Protected Properties      

        #endregion

        #region constructors

        static CachingProvider()
        {
            var providerConfig = Extensions.GetService<CacheProviderConfiguration>();
            if (providerConfig == null)
            {
                defaultCachingProvider = Extensions.GetService<MemoryCacheProvider>();
            }
            else
            {
                foreach (Provider objProvider in providerConfig.Providers.Values)
                {
                    if (objProvider.ProviderLevel != null || objProvider.Name == providerConfig.DefaultProvider)
                    {
                        Type objType = Type.GetType(objProvider.Type, true, true);

                        var objCachingProvider = (CachingProvider)Extensions.GetService(objType);

                        if (objProvider.Name == providerConfig.DefaultProvider)
                            defaultCachingProvider = objCachingProvider;
                        if (objProvider.ProviderLevel != null)
                            providerInstances.Add((ProviderLevel)Enum.Parse(typeof(ProviderLevel), objProvider.ProviderLevel), objCachingProvider);
                        foreach (var attrName in objProvider.Attributes.AllKeys)
                        {
                            var attrValue = objProvider.Attributes[attrName];
                            var propInfo = objType.GetProperty(attrName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                            if (propInfo != null)
                            {
                                object objValue = Convert.ChangeType(attrValue, propInfo.PropertyType);
                                propInfo.SetValue(objCachingProvider, objValue, null);
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region Shared/Static Methods

        /// <summary>
        /// Gets the cache key with key prefix.
        /// </summary>
        /// <param name="cacheKey">The cache key.</param>
        /// <param name="scope">The scope.</param>
        /// <returns>CachePrefix + Scope + CacheKey</returns>
        /// <exception cref="ArgumentException">Cache key is empty.</exception>
        public static string GetCacheKey(string cacheKey, string scope = null)
        {
            if (string.IsNullOrEmpty(cacheKey))
            {
                throw new ArgumentException("Argument cannot be null or an empty string", "CacheKey");
            }
            var strScope = scope == null ? "" : scope + "_";
            return CachePrefix + strScope + cacheKey;
        }

        public static string GetCacheKeyPrefix(string scope = null)
        {
            var strScope = scope == null ? "" : scope + "_";
            return CachePrefix + strScope;
        }

        /// <summary>
        /// Instances of  caching provider.
        /// </summary>
        /// <returns>The Implemments provider of cache system defind in config.</returns>
        public static CachingProvider Instance(ProviderLevel Level)
        {
            if (providerInstances.ContainsKey(Level))
            {
                return providerInstances[Level];
            }
            return defaultCachingProvider;
        }

        public static void RemoveCache(string cacheKey, string scope = null)
        {
            if (providerInstances.Count > 0)
            {
                foreach (var objProvider in providerInstances.Values)
                {
                    objProvider.Remove(cacheKey, scope);
                }
            }
            else
            {
                defaultCachingProvider.Remove(cacheKey, scope);
            }
        }
        public static void ClearCache()
        {
            if (providerInstances.Count > 0)
            {
                foreach (var objProvider in providerInstances.Values)
                {
                    objProvider.Clear();
                }
            }
            else
            {
                defaultCachingProvider.Clear();
            }
        }
        public static void ClearCache(string scope)
        {
            if (providerInstances.Count > 0)
            {
                foreach (var objProvider in providerInstances.Values)
                {
                    objProvider.Clear(scope);
                }
            }
            else
            {
                defaultCachingProvider.Clear(scope);
            }
        }
        #endregion

        #region Private Methods


        #endregion

        #region Public Methods

        /// <summary>
        /// Clears the specified scope.
        /// </summary>
        /// <param name="scope">The scope.</param>
        public abstract void Clear(string scope = null);

        /// <summary>
        /// Gets the item.
        /// </summary>
        /// <param name="cacheKey">The cache key.</param>
        /// <returns>cache content</returns>
        public abstract T GetItem<T>(string cacheKey, string scope = null);

        public abstract bool IsDistributedCache();

        /// <summary>
        /// Inserts the specified cache key.
        /// </summary>
        /// <param name="cacheKey">The cache key.</param>
        /// <param name="itemToCache">The value.</param>
        /// <param name="scope">The value in which scope.</param>
        /// <param name="absoluteExpiration">The absolute expiration.</param>
        /// <param name="slidingExpiration">The sliding expiration.</param>
        /// <param name="dependency">The dependency.</param>
        /// <param name="onRemoveCallback">The on remove callback.</param>
        public abstract void Insert(string cacheKey, object itemToCache, string scope = null, DateTimeOffset? absoluteExpiration = null, TimeSpan? slidingExpiration = null, FileCacheDependency dependency = null,
                                   RemoveDelegate onRemoveCallback = null);

        public abstract void Remove(string cacheKey, string scope = null);

        public abstract long GetListCount<T>(string listName);

        public abstract T GetItemFromList<T>(string listName, int listIndex);

        public abstract void SetItemInList<T>(string listName, int listIndex, T value);

        public abstract void EnqueueList<T>(string listName, T value);

        public abstract T DequeueList<T>(string listName);

        public abstract void RemoveFromList<T>(string listName, T value);

        public abstract void RemoveList(string listName);

        public abstract void SetEntryInHash<TKey, T>(string hashId, TKey key, T value);

        public abstract bool RemoveEntryFromHash<TKey>(string hashId, TKey key);

        public abstract T GetValueFromHash<TKey, T>(string hashId, TKey key);

        public abstract long IncrementValue(string key, int count, int expiredMinutes, Func<long> initialCallBack);

        public abstract long DecrementValue(string key, int count, int expiredMinutes, Func<long> initialCallBack);

        #endregion

    }
}