using Moq;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AutoGuru.KeyValuePush.Redis.Tests
{
    public class RedisPusherTests
    {
        private readonly Mock<IDatabase> _mockDatabase;
        private readonly RedisPusher _pusher;

        public RedisPusherTests()
        {
            _mockDatabase = new Mock<IDatabase>();
            _pusher = new RedisPusher();

            // Usamos reflexión para inyectar el mock en el campo privado
            var field = typeof(RedisPusher).GetField("_db", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(_pusher, _mockDatabase.Object);
        }

        
    }
}
