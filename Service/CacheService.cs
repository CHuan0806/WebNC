using StackExchange.Redis;

public class CacheService
{
    private readonly IDatabase _redisDb;

    public CacheService(IConnectionMultiplexer redis)
    {
        _redisDb = redis.GetDatabase();
    }

    public async Task SetAsync(string key, string value, TimeSpan? expiry = null)
    {
        if (expiry.HasValue && expiry.Value == TimeSpan.Zero)
        {
            Console.WriteLine($"⚠️ Lỗi: SetAsync được gọi với TimeSpan.Zero cho key: {key}");
            // throw nếu muốn phát hiện rõ tại đây
            throw new Exception($"Invalid expiry time (0s) for key: {key}");
        }

        await _redisDb.StringSetAsync(key, value, expiry ?? TimeSpan.FromMinutes(10));
    }


    public async Task<string> GetAsync(string key)
    {
        return await _redisDb.StringGetAsync(key);
    }
    public async Task RemoveAsync(string key)
    {
        await _redisDb.KeyDeleteAsync(key);
    }
}
