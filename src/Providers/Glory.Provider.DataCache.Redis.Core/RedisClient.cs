using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Glory.Provider.DataCache.Redis.Core
{
    public class RedisClient : IDisposable
    {
        private readonly IConnectionMultiplexer connectionMultiplexer;
        public RedisClient(ConfigurationOptions options, int database = 0)
        {
            connectionMultiplexer = ConnectionMultiplexer.Connect(options);

            Database = connectionMultiplexer.GetDatabase(database);
        }

        public IDatabase Database { get; }

        public void Dispose()
        {
            connectionMultiplexer.Dispose();
        }

        public T Get<T>(string key)
        {
            var valueBytes = Database.StringGet(key, CommandFlags.PreferSlave);

            if (!valueBytes.HasValue)
            {
                return default(T);
            }

            return JsonConvert.DeserializeObject<T>(valueBytes);
        }

        public bool Add<T>(string key, T value)
        {
            var entryBytes = JsonConvert.SerializeObject(value);

            return Database.StringSet(key, entryBytes);
        }

        public bool Add<T>(string key, T value, DateTimeOffset expiresAt)
        {
            var strVal = JsonConvert.SerializeObject(value);
            var expiration = expiresAt.Subtract(DateTimeOffset.Now);

            return Database.StringSet(key, strVal, expiration);
        }

        public bool Add<T>(string key, T value, TimeSpan expiresIn)
        {
            var strVal = JsonConvert.SerializeObject(value);

            return Database.StringSet(key, strVal, expiresIn);
        }

        public bool Remove(string key)
        {
            return Database.KeyDelete(key);
        }

        public long GetListCount(string listName)
        {
            return Database.ListLength(listName, CommandFlags.PreferSlave);
        }

        public T GetItemFromList<T>(string listName, int listIndex)
        {
            var objValue = Database.ListGetByIndex(listName, listIndex, CommandFlags.PreferSlave);
            if (!objValue.HasValue)
            {
                return default(T);
            }
            return JsonConvert.DeserializeObject<T>(objValue);
        }

        public void SetItemInList<T>(string listName, int listIndex, T value)
        {
            var strVal = JsonConvert.SerializeObject(value);
            Database.ListSetByIndex(listName, listIndex, strVal);
        }

        public long EnqueueList<T>(string listName, T value)
        {
            var strVal = JsonConvert.SerializeObject(value);
            return Database.ListLeftPush(listName, strVal);
        }

        public T DequeueList<T>(string listName)
        {
            var objVal = Database.ListRightPop(listName);
            return JsonConvert.DeserializeObject<T>(objVal);
        }

        public void RemoveFromList<T>(string listName, T value)
        {
            var strVal = JsonConvert.SerializeObject(value);
            Database.ListRemove(listName, strVal);
        }

        public long GetHashCount(string hashId)
        {
            return Database.HashLength(hashId, CommandFlags.PreferSlave);
        }

        public T GetValueFromHash<TKey, T>(string hashId, TKey key)
        {
            var strKey = JsonConvert.SerializeObject(key);

            var objVal = Database.HashGet(hashId, strKey, CommandFlags.PreferSlave);

            if (!objVal.HasValue)
                return default(T);

            return JsonConvert.DeserializeObject<T>(objVal);
        }

        public bool SetEntryInHash<TKey, T>(string hashId, TKey key, T value)
        {
            var strKey = JsonConvert.SerializeObject(key);
            var strVal = JsonConvert.SerializeObject(value);

            var entries = new HashEntry[] { new HashEntry(strKey, strVal) };
            Database.HashSet(hashId, entries);
            return true;
        }

        public bool RemoveEntryFromHash<TKey>(string hashId, TKey key)
        {
            var strKey = JsonConvert.SerializeObject(key);

            return Database.HashDelete(hashId, strKey);
        }

        public long IncrementValueInHash(string hashId, string key, int count)
        {
            return Database.HashIncrement(hashId, key, count);
        }

        public long DecrementValueInHash(string hashId, string key, int count)
        {
            return Database.HashDecrement(hashId, key, count);
        }

        public long IncrementValueBy(string key, int count)
        {
            return Database.StringIncrement(key, count);
        }

        public long DecrementValueBy(string key, int count)
        {
            return Database.StringDecrement(key, count);
        }

        public List<T> Sort<T>(string collectionKey, string byField, bool fieldIsNumber, int skip, int take, bool isAscending)
        {
            var items = Database.Sort(collectionKey, skip, take
                , order: isAscending ? Order.Ascending : Order.Descending
                , sortType: fieldIsNumber ? SortType.Numeric : SortType.Alphabetic
                , by: byField
                , flags: CommandFlags.PreferSlave
                );
            if (items == null)
            {
                return new List<T>(0);
            }
            else
            {
                var list = new List<T>(items.Length);
                foreach (var itm in items)
                {
                    var obj = JsonConvert.DeserializeObject<T>(itm);
                    list.Add(obj);
                }
                return list;
            }
        }

        public void RemoveAll(IEnumerable<string> keys)
        {
            var redisKeys = keys.Select(x => (RedisKey)x).ToArray();
            Database.KeyDelete(redisKeys);
        }

        public RedisLock AcquireLock(string key, TimeSpan timeout)
        {
            return new RedisLock(Database, key, timeout);
        }

        public IEnumerable<string> SearchKeys(string pattern)
        {
            var keys = new HashSet<RedisKey>();

            var server = connectionMultiplexer.GetServer(connectionMultiplexer.GetEndPoints().First());

            var dbKeys = server.Keys(Database.Database, pattern, flags: CommandFlags.PreferSlave);
            foreach (var dbKey in dbKeys)
            {
                if (!keys.Contains(dbKey))
                {
                    keys.Add(dbKey);
                }
            }

            return keys.Select(x => (string)x);
        }
    }
}
