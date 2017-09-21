
namespace Glory.Services.Core.DataCache.Parameters
{
    /// -----------------------------------------------------------------------------
    /// Class:      CacheItemExpiredCallback
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The CacheItemExpiredCallback delegate defines a callback method that notifies
    /// the application when a CacheItem is Expired (when an attempt is made to get the item)
    /// </summary>
    /// -----------------------------------------------------------------------------
    public delegate T CacheItemExpiredCallback<T>(CacheItemArgs dataArgs);
    public delegate void RemoveDelegate(object key, object value, RemoveReason reason, object state);

}
