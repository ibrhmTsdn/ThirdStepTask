using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Text.Json;
using ThirdStepTask.Product.Application.Services;

namespace ThirdStepTask.Product.Infrastructure.Services
{
    /// <summary>
    /// Redis cache service implementation
    /// Implements ICacheService using StackExchange.Redis
    /// </summary>
    public class RedisCacheService : ICacheService
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IDatabase _database;
        private readonly ILogger<RedisCacheService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public RedisCacheService(
            IConnectionMultiplexer redis,
            ILogger<RedisCacheService> logger)
        {
            _redis = redis;
            _database = redis.GetDatabase();
            _logger = logger;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };
        }

        public async Task<T?> GetAsync<T>(string key) where T : class
        {
            try
            {
                var value = await _database.StringGetAsync(key);

                if (value.IsNullOrEmpty)
                {
                    return null;
                }

                return JsonSerializer.Deserialize<T>(value!, _jsonOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting value from cache for key: {Key}", key);
                return null;
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class
        {
            try
            {
                var jsonValue = JsonSerializer.Serialize(value, _jsonOptions);
                await _database.StringSetAsync(key, jsonValue, expiration);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting value in cache for key: {Key}", key);
            }
        }

        public async Task RemoveAsync(string key)
        {
            try
            {
                await _database.KeyDeleteAsync(key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing value from cache for key: {Key}", key);
            }
        }

        public async Task RemoveByPrefixAsync(string prefix)
        {
            try
            {
                var server = _redis.GetServer(_redis.GetEndPoints().First());
                var keys = server.Keys(pattern: $"{prefix}*").ToArray();

                if (keys.Length > 0)
                {
                    await _database.KeyDeleteAsync(keys);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing values from cache by prefix: {Prefix}", prefix);
            }
        }

        public async Task<bool> ExistsAsync(string key)
        {
            try
            {
                return await _database.KeyExistsAsync(key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if key exists in cache: {Key}", key);
                return false;
            }
        }

        public async Task<Dictionary<string, T?>> GetManyAsync<T>(IEnumerable<string> keys) where T : class
        {
            var result = new Dictionary<string, T?>();

            try
            {
                var redisKeys = keys.Select(k => (RedisKey)k).ToArray();
                var values = await _database.StringGetAsync(redisKeys);

                for (int i = 0; i < redisKeys.Length; i++)
                {
                    var key = redisKeys[i].ToString();
                    if (!values[i].IsNullOrEmpty)
                    {
                        result[key] = JsonSerializer.Deserialize<T>(values[i]!, _jsonOptions);
                    }
                    else
                    {
                        result[key] = null;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting multiple values from cache");
            }

            return result;
        }

        public async Task SetManyAsync<T>(Dictionary<string, T> keyValuePairs, TimeSpan? expiration = null) where T : class
        {
            try
            {
                var batch = _database.CreateBatch();
                var tasks = new List<Task>();

                foreach (var kvp in keyValuePairs)
                {
                    var jsonValue = JsonSerializer.Serialize(kvp.Value, _jsonOptions);
                    tasks.Add(batch.StringSetAsync(kvp.Key, jsonValue, expiration));
                }

                batch.Execute();
                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting multiple values in cache");
            }
        }
    }
}
