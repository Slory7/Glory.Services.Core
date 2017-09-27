using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Glory.Services.Core.Common
{
    public static class UniqueObject
    {
        private static readonly ReaderWriterLockSlim dictionaryLock = new ReaderWriterLockSlim();
        private static readonly Dictionary<string, object> uniqueDictionary = new Dictionary<string, object>();

        #region Public Static Methods

        public static T GetUniqueObject<T>(string key) where T : new()
        {
            object @lock = null;
            if (dictionaryLock.TryEnterReadLock(new TimeSpan(0, 0, 5)))
            {
                try
                {
                    //Try to get lock Object (for key) from Dictionary
                    if (uniqueDictionary.ContainsKey(key))
                    {
                        @lock = uniqueDictionary[key];
                    }
                }
                finally
                {
                    dictionaryLock.ExitReadLock();
                }
            }
            if (@lock == null)
            {
                if (dictionaryLock.TryEnterWriteLock(new TimeSpan(0, 0, 5)))
                {
                    try
                    {
                        //Double check dictionary
                        if (!uniqueDictionary.ContainsKey(key))
                        {
                            //Create new lock
                            uniqueDictionary[key] = new T();
                        }
                        //Retrieve lock
                        @lock = uniqueDictionary[key];
                    }
                    finally
                    {
                        dictionaryLock.ExitWriteLock();
                    }
                }
            }
            return (T)@lock;
        }

        public static void RemoveUniqueObject(string key)
        {
            if (dictionaryLock.TryEnterWriteLock(new TimeSpan(0, 0, 5)))
            {
                try
                {
                    //check dictionary
                    if (uniqueDictionary.ContainsKey(key))
                    {
                        //Remove lock
                        uniqueDictionary.Remove(key);
                    }
                }
                finally
                {
                    dictionaryLock.ExitWriteLock();
                }
            }
        }

        #endregion

    }
}
