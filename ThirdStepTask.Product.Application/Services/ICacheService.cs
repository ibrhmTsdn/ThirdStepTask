namespace ThirdStepTask.Product.Application.Services
{
    /// <summary>
    /// Cache service interface for Redis operations
    /// Follows Interface Segregation Principle
    /// </summary>
    public interface ICacheService
    {
        /// <summary>
        /// Gets a value from cache
        /// </summary>
        Task<T?> GetAsync<T>(string key) where T : class;

        /// <summary>
        /// Sets a value in cache with expiration
        /// </summary>
        Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class;

        /// <summary>
        /// Removes a value from cache
        /// </summary>
        Task RemoveAsync(string key);

        /// <summary>
        /// Removes all keys matching a pattern
        /// </summary>
        Task RemoveByPrefixAsync(string prefix);

        /// <summary>
        /// Checks if a key exists in cache
        /// </summary>
        Task<bool> ExistsAsync(string key);

        /// <summary>
        /// Gets multiple values from cache
        /// </summary>
        Task<Dictionary<string, T?>> GetManyAsync<T>(IEnumerable<string> keys) where T : class;

        /// <summary>
        /// Sets multiple values in cache
        /// </summary>
        Task SetManyAsync<T>(Dictionary<string, T> keyValuePairs, TimeSpan? expiration = null) where T : class;
    }
}
