
#region Usings

using System.Collections;
using Glory.Services.Core.Config;

#endregion

namespace Glory.Services.Core.DataCache.Parameters
{
    /// -----------------------------------------------------------------------------  
    /// Class:      CacheItemArgs
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The CacheItemArgs class provides an EventArgs implementation for the
    /// CacheItemExpiredCallback delegate
    /// </summary>   
    /// -----------------------------------------------------------------------------
    public class CacheItemArgs
    {
        private ArrayList _paramList;

        ///-----------------------------------------------------------------------------
        /// <summary>
        /// Constructs a new CacheItemArgs Object
        /// </summary>
        /// <param name="key"></param>
        ///-----------------------------------------------------------------------------
        public CacheItemArgs(string key)
            : this(key, 20, 0, ProviderLevel.Normal, null)
        {
        }

        ///-----------------------------------------------------------------------------
        /// <summary>
        /// Constructs a new CacheItemArgs Object
        /// </summary>
        /// <param name="key"></param>
        /// <param name="timeout"></param>
        ///-----------------------------------------------------------------------------
        public CacheItemArgs(string key, int timeoutMinutes)
            : this(key, timeoutMinutes, 0, ProviderLevel.Normal, null)
        {
        }
        public CacheItemArgs(string key, int timeoutMinutes, int expireTimeoutMinutes)
            : this(key, timeoutMinutes, expireTimeoutMinutes, ProviderLevel.Normal, null)
        {
        }
        ///-----------------------------------------------------------------------------
        /// <summary>
        /// Constructs a new CacheItemArgs Object
        /// </summary>
        /// <param name="key"></param>
        /// <param name="level"></param>
        ///-----------------------------------------------------------------------------
        public CacheItemArgs(string key, ProviderLevel level)
            : this(key, 20, 0, level, null)
        {
        }

        ///-----------------------------------------------------------------------------
        /// <summary>
        /// Constructs a new CacheItemArgs Object
        /// </summary>
        /// <param name="key"></param>
        /// <param name="timeoutMinutes"></param>
        /// <param name="level"></param>
        ///-----------------------------------------------------------------------------
        public CacheItemArgs(string key, int timeoutMinutes, ProviderLevel level)
            : this(key, timeoutMinutes, 0, level, null)
        {
        }
        public CacheItemArgs(string key, int timeoutMinutes, int expireTimeoutMinutes, ProviderLevel level)
            : this(key, timeoutMinutes, expireTimeoutMinutes, level, null)
        {
        }
        ///-----------------------------------------------------------------------------
        /// <summary>
        /// Constructs a new CacheItemArgs Object
        /// </summary>
        /// <param name="key"></param>
        /// <param name="timeout"></param>
        /// <param name="level"></param>
        /// <param name="parameters"></param>
        ///-----------------------------------------------------------------------------
        public CacheItemArgs(string key, int timeout, int expireTimeout, ProviderLevel level, params object[] parameters)
        {
            CacheKey = key;
            CacheTimeOutMinutes = timeout;
            ExpireTimeOutMinutes = expireTimeout;
            Level = level;
            Params = parameters;
        }

        ///-----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Cache Item's CacheItemRemovedCallback delegate
        /// </summary>
        ///-----------------------------------------------------------------------------
        public RemoveDelegate CacheCallback { get; set; }

        ///-----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Cache Item's CacheDependency
        /// </summary>
        /// <history>
        ///     [cnurse]	01/12/2008	created
        /// </history>
        ///-----------------------------------------------------------------------------
        public FileCacheDependency CacheDependency { get; set; }

        ///-----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Cache Item's Key
        /// </summary>
        /// <history>
        ///     [cnurse]	01/12/2008	created
        /// </history>
        ///-----------------------------------------------------------------------------
        public string CacheKey { get; set; }

        ///-----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Cache Item's level (defaults to Default)
        /// </summary>
        /// <remarks>
        /// ItemPriority, but this is included for possible future use. </remarks>
        ///-----------------------------------------------------------------------------
        public ProviderLevel Level { get; set; }

        public string Scope { get; set; }
        ///-----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Cache Item's Timeout
        /// </summary>
        ///-----------------------------------------------------------------------------
        public int CacheTimeOutMinutes { get; set; }

        /// <summary>
        /// Absolute Expire TimeOut Minute
        /// </summary>
        public int ExpireTimeOutMinutes { get; set; }
        ///-----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Cache Item's Parameter List
        /// </summary>
        ///-----------------------------------------------------------------------------
        public ArrayList ParamList
        {
            get
            {
                if (_paramList == null)
                {
                    _paramList = new ArrayList();
                    //add additional params to this list if its not null
                    if (Params != null)
                    {
                        foreach (object param in Params)
                        {
                            _paramList.Add(param);
                        }
                    }
                }

                return _paramList;
            }
        }

        ///-----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Cache Item's Parameter Array
        /// </summary>
        ///-----------------------------------------------------------------------------
        public object[] Params { get; private set; }
    }
}
