using StackExchange.Redis;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AutoGuru.KeyValuePush.Redis
{
    public class RedisPusher : IPusher
    {
        public async Task PushAsync(
            string redisConfiguration,
            int? redisDb,
            IDictionary<string, string> dictionary,
            CancellationToken cancellationToken)
        {
            using var connectionMultiplexer = ConnectionMultiplexer.Connect(redisConfiguration);
            var db = redisDb.HasValue 
                ? connectionMultiplexer.GetDatabase(redisDb.Value)
                : connectionMultiplexer.GetDatabase();

            await db.StringSetAsync(dictionary
                .ToDictionary(kvp => new RedisKey(kvp.Key), kvp => new RedisValue(kvp.Value))
                .ToArray());
        }
    }
}
