using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;
using VtodoManager.NewsService.Infrastructure.Interfaces.Services;

namespace VtodoManager.NewsService.Infrastructure.Implementation.Services;

internal class RedisKeysUtilsService : IRedisKeysUtilsService
{
    private readonly IDistributedCache _distributedCache;
    private readonly IConnectionMultiplexer _connectionMultiplexer;

    public RedisKeysUtilsService(
        IDistributedCache distributedCache,
        IConnectionMultiplexer connectionMultiplexer)
    {
        _distributedCache = distributedCache;
        _connectionMultiplexer = connectionMultiplexer;
    }
    
    public async Task RemoveKeysByKeyroot(string keyRoot)
    {
        if (string.IsNullOrWhiteSpace(keyRoot))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(keyRoot));

        // get all the keys* and remove each one
        await foreach (var key in GetKeysAsync(keyRoot + "*"))
        {
            await _distributedCache.RemoveAsync(key);
        }
    }
    
    public async IAsyncEnumerable<string> GetKeysAsync(string pattern)
    {
        if (string.IsNullOrWhiteSpace(pattern))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(pattern));

        foreach (var endpoint in _connectionMultiplexer.GetEndPoints())
        {
            var server = _connectionMultiplexer.GetServer(endpoint);
            await foreach (var key in server.KeysAsync(pattern: pattern))
            {
                yield return key.ToString();
            }
        }
    }
}