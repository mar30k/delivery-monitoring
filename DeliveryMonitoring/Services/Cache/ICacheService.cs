namespace DeliveryMonitoring.Services.Cache
{
    public interface ICacheService
    {
        Task<T> GetOrSetAsync<T>(
            string key,
            Func<Task<T>> fetchFunc,
            bool skipCache = false,
            int cacheMinutes = 10);

        void Remove(string key);
    }
}
