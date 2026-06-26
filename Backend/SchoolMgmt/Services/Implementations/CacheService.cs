using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using SchoolMgmt.Services.Interfaces;
using SchoolMgmt.Settings;

namespace SchoolMgmt.Services.Implementations;

public class CacheService : ICacheService
{
    private readonly IDistributedCache _cache;
    private readonly RedisSettings _settings;

    // In-memory key tracking (for prefix removal)
    private static readonly HashSet<string> _keys = new();

    public CacheService(IDistributedCache cache, IOptions<RedisSettings> settings)
    {
        _cache = cache;
        _settings = settings.Value;
    }

    public async Task<T?> GetAsync<T>(string key) where T : class
    {
        try
        {
            var data = await _cache.GetStringAsync(key);
            return data == null
                ? null
                : JsonSerializer.Deserialize<T>(data);
        }
        catch
        {
            return null; // Cache failure should not break the API
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null) where T : class
    {
        try
        {
            var ttl = expiry ?? TimeSpan.FromMinutes(_settings.DefaultTtlMinutes);
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = ttl
            };
            var json = JsonSerializer.Serialize(value);
            await _cache.SetStringAsync(key, json, options);
            lock (_keys) { _keys.Add(key); }
        }
        catch { }
    }

    public async Task RemoveAsync(string key)
    {
        try
        {
            await _cache.RemoveAsync(key);
            lock (_keys) { _keys.Remove(key); }
        }
        catch { }
    }

    public async Task RemoveByPrefixAsync(string prefix)
    {
        try
        {
            List<string> keysToRemove;
            lock (_keys)
            {
                keysToRemove = _keys
                    .Where(k => k.StartsWith(prefix))
                    .ToList();
            }

            foreach (var key in keysToRemove)
                await RemoveAsync(key);
        }
        catch { }
    }
}