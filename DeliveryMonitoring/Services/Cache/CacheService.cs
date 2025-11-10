using Microsoft.Extensions.Caching.Memory;

namespace DeliveryMonitoring.Services.Cache
{
    public class CacheService : ICacheService
    {
        private readonly IMemoryCache _memoryCache;
        public CacheService(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> fetchFunc, bool skipCache = false, int cacheMinutes = 10)
        {
            if (skipCache)
            {
                var fresh = await fetchFunc();
                _memoryCache.Set(key, fresh, TimeSpan.FromMinutes(cacheMinutes));
                return fresh;
            }

            if (_memoryCache.TryGetValue(key, out T cachedValue))
                return cachedValue;

            var result = await fetchFunc();
            _memoryCache.Set(key, result, TimeSpan.FromMinutes(cacheMinutes));

            return result;
        }

        public void Remove(string key)
        {
            _memoryCache.Remove(key);
        }
    }
}
