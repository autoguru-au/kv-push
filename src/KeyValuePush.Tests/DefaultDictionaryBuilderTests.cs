using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AutoGuru.KeyValuePush.Tests;

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
        const string key = "key1";
        const string value = "value1";

        DefaultDictionaryBuilder.TryAdd(dict, key, value);

        Assert.Single(dict);
        Assert.Equal(value, dict[key]);
    }

    [Fact]
    public void TryAdd_ShouldNotThrow_WhenKeyExistsWithSameValue()
    {
        var dict = new Dictionary<string, string> { { "key1", "value1" } };
        const string key = "key1";
        const string value = "value1";

        DefaultDictionaryBuilder.TryAdd(dict, key, value);

        Assert.Single(dict);
        Assert.Equal(value, dict[key]);
    }

    [Fact]
    public void TryAdd_ShouldThrowException_WhenKeyExistsWithDifferentValue()
    {
        var dict = new Dictionary<string, string> { { "key1", "value1" } };
        const string key = "key1";
        const string value = "differentValue";

        var exception = Assert.Throws<InvalidOperationException>(() => DefaultDictionaryBuilder.TryAdd(dict, key, value));
        Assert.Equal("Duplicate key of 'key1' with a different value detected.", exception.Message);
    }

    [Fact]
    public async Task BuildAsync_ShouldThrowException_WhenFileAccessFailsAsync()
    {
        const string path = "RestrictedFiles";
        Directory.CreateDirectory(path);
        var filePath = Path.Combine(path, "file1.txt");
        await File.WriteAllTextAsync(filePath, "Content1");

        await using (File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.None))
        {
            var exception = await Assert.ThrowsAsync<IOException>(() =>
                _builder.BuildAsync(path, "*.txt", SearchOption.TopDirectoryOnly, false, CancellationToken.None));

            Assert.Contains("being used by another process", exception.Message);
        }

        File.Delete(filePath);
        Directory.Delete(path);
    }

    [Fact]
    public async Task BuildAsync_ShouldRespectCancellationTokenAsync()
    {
        const string path = "TestFiles";
        Directory.CreateDirectory(path);
        await File.WriteAllTextAsync(Path.Combine(path, "file1.txt"), "Content1");

        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            _builder.BuildAsync(path, "*.txt", SearchOption.TopDirectoryOnly, false, cts.Token));

        Directory.Delete(path, true);
    }

    [Fact]
    public async Task BuildAsync_ShouldHandleDuplicateKeysInJsonFilesAsync()
    {
        const string path = "TestFiles";
        Directory.CreateDirectory(path);

        var jsonContent = JsonSerializer.Serialize(new Dictionary<string, string>
        {
            { "Key1", "Value1" },
            { "Key2", "Value2" }
        });
        await File.WriteAllTextAsync(Path.Combine(path, "file1.json"), jsonContent);

        var result = await _builder.BuildAsync(path, "*.json", SearchOption.TopDirectoryOnly, true, CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.Equal("Value1", result["Key1"]);
        Assert.Equal("Value2", result["Key2"]);

        Directory.Delete(path, true);
    }

    [Fact]
    public async Task BuildAsync_ShouldIgnoreFilesWithNonJsonExtensionsAsync()
    {
        const string path = "TestFiles";
        Directory.CreateDirectory(path);

        await File.WriteAllTextAsync(Path.Combine(path, "file1.unsupported"), "Unsupported Content");

        var result = await _builder.BuildAsync(path, "*.json", SearchOption.TopDirectoryOnly, false, CancellationToken.None);

        Assert.Empty(result);
        Directory.Delete(path, true);
    }

    [Fact]
    public async Task BuildAsync_ShouldCombineJsonAndTextFilesAsync()
    {
        const string path = "TestFiles";
        Directory.CreateDirectory(path);
        var jsonContent = JsonSerializer.Serialize(new Dictionary<string, string>
        {
            { "Key1", "Value1" },
            { "Key2", "Value2" }
        });

        await File.WriteAllTextAsync(Path.Combine(path, "file1.json"), jsonContent);
        await File.WriteAllTextAsync(Path.Combine(path, "file2.txt"), "TextContent");

        var result = await _builder.BuildAsync(path, "*.*", SearchOption.TopDirectoryOnly, true, CancellationToken.None);

        Assert.Equal(3, result.Count);
        Assert.Equal("Value1", result["Key1"]);
        Assert.Equal("Value2", result["Key2"]);
        Assert.Equal("TextContent", result["file2"]);
        Directory.Delete(path, true);
    }
}
