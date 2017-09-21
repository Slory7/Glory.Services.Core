using StackExchange.Redis;
using System;

namespace Glory.Provider.DataCache.Redis.Core
{
    public class RedisLock : IDisposable
    {
        readonly IDatabase _database;
        readonly string _key;
        public RedisLock(IDatabase db, string key, TimeSpan timeout)
        {
            _database = db;
            _key = key;
            _database.LockTake(key, "1", timeout);
        }
        public void Dispose()
        {
            _database.LockRelease(_key, "1");
        }
    }
}
