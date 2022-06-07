namespace PrbCacheWrapper
{
    public class CacheStampedeStore : ICacheStampedeStore
    {
        public string Read(int id)
        {
            return $"Value {id}";
        }
    }
}
