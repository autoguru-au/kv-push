using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using Moq;
using StackExchange.Redis;
using Xunit;

namespace AutoGuru.KeyValuePush.Redis.Tests;

public class RedisPusherTests
{
    private readonly Mock<IDatabase> _mockDatabase;
    private readonly RedisPusher _pusher;

    public RedisPusherTests()
    {
        _mockDatabase = new Mock<IDatabase>();
        _pusher = new RedisPusher();

        var field = typeof(RedisPusher).GetField("_db", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        field?.SetValue(_pusher, _mockDatabase.Object);
    }
    
    [Fact]
    public async Task PushAsync_ShouldCallDatabaseStringSetAsync()
    {
        // Arrange
        var dictionary = new Dictionary<string, string>
        {
            { "key1", "value1" },
            { "key2", "value2" }
        };
        var cancellationToken = CancellationToken.None;

        _mockDatabase
            .Setup(db => db.StringSetAsync(It.IsAny<KeyValuePair<RedisKey, RedisValue>[]>(), When.Always, CommandFlags.None))
            .ReturnsAsync(true);

        // Act
        await _pusher.PushAsync(dictionary, cancellationToken);

        // Assert
        _mockDatabase.Verify(db => db.StringSetAsync(It.Is<KeyValuePair<RedisKey, RedisValue>[]>(kvps =>
            kvps.Length == dictionary.Count &&
            kvps[0].Key == "key1" && kvps[0].Value == "value1" &&
            kvps[1].Key == "key2" && kvps[1].Value == "value2"
        ), When.Always, CommandFlags.None), Times.Once);
    }

}
