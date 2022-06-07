using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using ProtoBuf;

namespace PrbCacheWrapper
{
    public class CacheStampedeRedis : ICacheStampedeRedis
    {
        private readonly Random _random = new Random();
        private readonly ICacheStampedeStore _store;
        private readonly IDistributedCache _distributedCache;

        public CacheStampedeRedis(ICacheStampedeStore store,
            IDistributedCache distributedCache)
        {
            _store = store;
            _distributedCache = distributedCache;
        }

        /// <summary>
        /// Attempts to read a value from the cache and uses probabilistic cache regeneration when necessary.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id">The ID to use when accessing the Store when regenerating data</param>
        /// <param name="cacheKey">The cache key</param>
        /// <param name="timeToLive">Time which the cached item should be alive for</param>
        /// <param name="beta">Setting value higher than 1 will favor early expire of cache</param>
        /// <returns></returns>
        public async Task<string> Fetch(int id, string cacheKey, TimeSpan timeToLive, byte beta = 1)
        {
            var byteData = await _distributedCache.GetAsync(cacheKey);

            CacheContainer<string> item = null;
            if (byteData != null)
            {
                item = JsonConvert.DeserializeObject<CacheContainer<string>>(Encoding.UTF8.GetString(byteData));
            }

            if (item != null)
            {
                double calc = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - item.Delta * beta * Math.Log(this._random.NextDouble());
                if (calc < item.ExpirationTime)
                {
                    return item.Value;
                }
            }

            // Start Measuring Delta
            Stopwatch sw = Stopwatch.StartNew();

            // Compute Value
            var value = this._store.Read(id);

            // Stop
            sw.Stop();

            // Add to Cache
            CacheContainer<string> cacheContainer = new CacheContainer<string>
            {
                Value = value,
                Delta = sw.ElapsedMilliseconds,
                ExpirationTime = DateTimeOffset.UtcNow.AddMilliseconds(timeToLive.TotalMilliseconds).ToUnixTimeMilliseconds()
            };

            // Extend cache expiry
            TimeSpan ttl = timeToLive.Add(TimeSpan.FromMilliseconds(cacheContainer.ExpirationTime - DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()));

            byteData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(cacheContainer));

            await _distributedCache.SetAsync(cacheKey,
                byteData, 
                new DistributedCacheEntryOptions()
                {
                    SlidingExpiration = ttl
                });

            // Return Value
            return value;
        }

    }

    [ProtoContract]
    class CacheContainer<T>
    {
        [ProtoMember(1)]
        internal T Value { get; set; }

        [ProtoMember(2)]
        internal long Delta { get; set; }

        [ProtoMember(3)]
        internal long ExpirationTime { get; set; }
    }
}
