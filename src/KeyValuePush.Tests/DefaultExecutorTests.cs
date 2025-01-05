using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AutoGuru.KeyValuePush;
using Moq;
using Xunit;

namespace KeyValuePush.Tests
{
    public class DefaultExecutorTests
    {
        private readonly Mock<IDictionaryBuilder> _mockDictionaryBuilder;
        private readonly Mock<IPusher> _mockPusher;
        private readonly DefaultExecutor _executor;

        public DefaultExecutorTests()
        {
            _mockDictionaryBuilder = new Mock<IDictionaryBuilder>();
            _mockPusher = new Mock<IPusher>();
            _executor = new DefaultExecutor(_mockDictionaryBuilder.Object, _mockPusher.Object);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldReturnZero_WhenSuccessfulAsync()
        {
            // Arrange
            var dict = new Dictionary<string, string> { { "key1", "value1" } };
            _mockDictionaryBuilder
                .Setup(builder => builder.BuildAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SearchOption>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(dict);
            _mockPusher
                .Setup(pusher => pusher.PushAsync(It.IsAny<IDictionary<string, string>>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _executor.ExecuteAsync("path", "*.txt", SearchOption.TopDirectoryOnly, false);

            // Assert
            Assert.Equal(0, result);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldReturnBuilderError_WhenBuildAsyncFailsAsync()
        {
            // Arrange
            _mockDictionaryBuilder
                .Setup(builder => builder.BuildAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SearchOption>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Build error"));

            // Act
            var result = await _executor.ExecuteAsync("path", "*.txt", SearchOption.TopDirectoryOnly, false);

            // Assert
            Assert.Equal((int)DefaultExecutor.ErrorCode.BuilderError, result);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldReturnPusherError_WhenPushAsyncFailsAsync()
        {
            // Arrange
            var dict = new Dictionary<string, string> { { "key1", "value1" } };
            _mockDictionaryBuilder
                .Setup(builder => builder.BuildAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SearchOption>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(dict);
            _mockPusher
                .Setup(pusher => pusher.PushAsync(It.IsAny<IDictionary<string, string>>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Push error"));

            // Act
            var result = await _executor.ExecuteAsync("path", "*.txt", SearchOption.TopDirectoryOnly, false);

            // Assert
            Assert.Equal((int)DefaultExecutor.ErrorCode.PusherError, result);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldLogErrorMessage_WhenExceptionOccursAsync()
        {
            // Arrange
            var dict = new Dictionary<string, string> { { "key1", "value1" } };
            _mockDictionaryBuilder
                .Setup(builder => builder.BuildAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SearchOption>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(dict);
            _mockPusher
                .Setup(pusher => pusher.PushAsync(It.IsAny<IDictionary<string, string>>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Push error"));

            using var consoleOutput = new StringWriter();
            Console.SetError(consoleOutput);

            // Act
            await _executor.ExecuteAsync("path", "*.txt", SearchOption.TopDirectoryOnly, false);

            // Assert
            var output = consoleOutput.ToString();
            Assert.Contains("Unexpected error!", output);
            Assert.Contains("Push error", output);
        }
    }
}
