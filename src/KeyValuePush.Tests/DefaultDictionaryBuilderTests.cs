using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AutoGuru.KeyValuePush.Tests
{
    public class DefaultDictionaryBuilderTests
    {
        private readonly DefaultDictionaryBuilder _builder;

        public DefaultDictionaryBuilderTests()
        {
            _builder = new DefaultDictionaryBuilder();
        }

        [Fact]
        public void TryAdd_ShouldAddKeyValuePair_WhenKeyDoesNotExist()
        {
            var dict = new Dictionary<string, string>();
            var key = "key1";
            var value = "value1";

            DefaultDictionaryBuilder.TryAdd(dict, key, value);

            Assert.Single(dict);
            Assert.Equal(value, dict[key]);
        }

        [Fact]
        public void TryAdd_ShouldNotThrow_WhenKeyExistsWithSameValue()
        {
            var dict = new Dictionary<string, string> { { "key1", "value1" } };
            var key = "key1";
            var value = "value1";

            DefaultDictionaryBuilder.TryAdd(dict, key, value);

            Assert.Single(dict);
            Assert.Equal(value, dict[key]);
        }

        [Fact]
        public void TryAdd_ShouldThrowException_WhenKeyExistsWithDifferentValue()
        {
            var dict = new Dictionary<string, string> { { "key1", "value1" } };
            var key = "key1";
            var value = "differentValue";

            var exception = Assert.Throws<Exception>(() => DefaultDictionaryBuilder.TryAdd(dict, key, value));
            Assert.Equal("Duplicate key of 'key1' with a different value detected.", exception.Message);
        }

        [Fact]
        public async Task BuildAsync_ShouldReturnDictionary_WhenFilesAreValid()
        {
            var path = "TestFiles";
            Directory.CreateDirectory(path);
            File.WriteAllText(Path.Combine(path, "file1.txt"), "Content1");
            File.WriteAllText(Path.Combine(path, "file2.txt"), "Content2");

            var result = await _builder.BuildAsync(path, "*.txt", SearchOption.TopDirectoryOnly, false, CancellationToken.None);

            Assert.Equal(2, result.Count);
            Assert.Equal("Content1", result["file1"]);
            Assert.Equal("Content2", result["file2"]);

            Directory.Delete(path, true);
        }

        [Fact]
        public async Task BuildAsync_ShouldThrowJsonException_WhenJsonFileIsInvalid()
        {
            var path = "TestFiles";
            Directory.CreateDirectory(path);
            File.WriteAllText(Path.Combine(path, "file1.json"), "Invalid JSON Content");

            var exception = await Assert.ThrowsAsync<JsonException>(() => _builder.BuildAsync(path, "*.json", SearchOption.TopDirectoryOnly, true, CancellationToken.None));

            Assert.Contains("is an invalid start of a value", exception.Message);
            Directory.Delete(path, true);
        }

        [Fact]
        public async Task BuildAsync_ShouldHandleEmptyDirectory()
        {
            var path = "EmptyDirectory";
            Directory.CreateDirectory(path);

            var result = await _builder.BuildAsync(path, "*.*", SearchOption.TopDirectoryOnly, false, CancellationToken.None);

            Assert.Empty(result);
            Directory.Delete(path, true);
        }

        [Fact]
        public async Task BuildAsync_ShouldReturnEmptyDictionary_WhenNoMatchingFilesFound()
        {
            var path = "TestFiles";
            Directory.CreateDirectory(path);

            var result = await _builder.BuildAsync(path, "*.xml", SearchOption.TopDirectoryOnly, false, CancellationToken.None);

            Assert.Empty(result);
            Directory.Delete(path, true);
        }

        [Fact]
        public async Task BuildAsync_ShouldSkipJsonFiles_WhenRecurseIntoJsonFilesIsFalse()
        {
            var path = "TestFiles";
            Directory.CreateDirectory(path);
            var jsonContent = JsonSerializer.Serialize(new Dictionary<string, string>
            {
                { "Key1", "Value1" },
                { "Key2", "Value2" }
            });
            File.WriteAllText(Path.Combine(path, "file1.json"), jsonContent);

            var result = await _builder.BuildAsync(path, "*.json", SearchOption.TopDirectoryOnly, false, CancellationToken.None);

            Assert.Single(result);
            Assert.Equal(jsonContent, result["file1"]);

            Directory.Delete(path, true);
        }

        [Fact]
        public async Task BuildAsync_ShouldProcessNestedDirectories_WhenSearchOptionIsAllDirectories()
        {
            var path = "NestedTestFiles";
            var nestedPath = Path.Combine(path, "SubDirectory");
            Directory.CreateDirectory(nestedPath);
            File.WriteAllText(Path.Combine(nestedPath, "file1.txt"), "Content1");

            var result = await _builder.BuildAsync(path, "*.txt", SearchOption.AllDirectories, false, CancellationToken.None);

            Assert.Single(result);
            Assert.Equal("Content1", result["file1"]);

            Directory.Delete(path, true);
        }

        [Fact]
public async Task BuildAsync_ShouldIgnoreUnsupportedExtensions()
{
    // Arrange
    var path = "TestFiles";
    Directory.CreateDirectory(path);
    File.WriteAllText(Path.Combine(path, "file1.unsupported"), "Unsupported Content");

    // Act
    var result = await _builder.BuildAsync(path, "*.txt", SearchOption.TopDirectoryOnly, false, CancellationToken.None);

    // Assert
    Assert.Empty(result);

    // Clean up
    Directory.Delete(path, true);
}

    }


}
