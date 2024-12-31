using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AutoGuru.KeyValuePush.Redis
{
    public sealed class RedisPusher : IPusher, IDisposable
    {
        private ConnectionMultiplexer? _connectionMultiplexer;
        private IDatabase? _db;

        public void Configure(
            string configuration,
            int? db)
        {
            var configOptions = ConfigurationOptions.Parse(configuration);
            _connectionMultiplexer = ConnectionMultiplexer.Connect(configOptions);
            _db = db.HasValue
                ? _connectionMultiplexer.GetDatabase(db.Value)
                : _connectionMultiplexer.GetDatabase();
        }

        public async Task PushAsync(
            IDictionary<string, string> dictionary,
            CancellationToken cancellationToken)
        {
            if (_db is null)
            {
                throw new Exception($"{nameof(RedisPusher)} wasn't configured yet.");
            }

            await _db.StringSetAsync(dictionary
                .ToDictionary(kvp => new RedisKey(kvp.Key), kvp => new RedisValue(kvp.Value))
                .ToArray());
        }

        public void Dispose()
        {
            _connectionMultiplexer?.Dispose();
        }
    }
}
