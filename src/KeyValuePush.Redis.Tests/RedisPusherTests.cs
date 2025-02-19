using Moq;
using StackExchange.Redis;

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
}
