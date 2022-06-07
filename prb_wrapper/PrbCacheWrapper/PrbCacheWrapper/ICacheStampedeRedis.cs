namespace PrbCacheWrapper
{
    public interface ICacheStampedeRedis
    {
        Task<string> Fetch(int id, string cacheKey, TimeSpan timeToLive, byte beta = 1);
    }
}
