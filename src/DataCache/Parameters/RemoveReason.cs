namespace Glory.Services.Core.DataCache.Parameters
{
    public enum RemoveReason
    {
        None = 0,
        //     Manually
        Removed = 1,
        //     Overwritten
        Replaced = 2,
        //     Timed out
        Expired = 3,
        //     Event
        TokenExpired = 4,
        //     Overflow
        Capacity = 5
    }
}
