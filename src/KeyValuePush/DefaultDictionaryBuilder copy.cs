using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace AutoGuru.KeyValuePush
{
    public class DefaultDictionaryBuilder : IDictionaryBuilder
    {
        public async Task<IDictionary<string, string>> BuildAsync(
            string path,
            string searchPattern,
            SearchOption searchOption,
            bool recurseIntoJsonFiles,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var filePaths = Directory.GetFiles(path, searchPattern, searchOption);
            var dict = new Dictionary<string, string>(filePaths.Length);
            
            foreach (var filePath in filePaths)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (Path.GetExtension(filePath).Contains(".json") &&
                    recurseIntoJsonFiles)
                {
                    // Read file as JSON and extract kvps
                    var bytes = await File.ReadAllBytesAsync(filePath, cancellationToken);
                    var innerDict = JsonSerializer.Deserialize<Dictionary<string, string>>(bytes);
                    if (innerDict is null)
                    {
                        throw new Exception($"Problem reading {filePath} as dictionary.");
                    }

                    foreach (var kvp in innerDict)
                    {
                        TryAdd(dict, kvp.Key, kvp.Value);
                    }
                }
                else
                {
                    var fileName = Path.GetFileNameWithoutExtension(filePath);
                    var text = await File.ReadAllTextAsync(filePath, cancellationToken);
                    TryAdd(dict, fileName, text);
                }
            }

            return dict;
        }

        public static void TryAdd(IDictionary<string, string> dict, string key, string value)
        {
            if (dict.ContainsKey(key))
            {
                if (dict[key] == value)
                {
                    return;
                }

                throw new Exception($"Duplicate key of '{key}' with a different value detected.");
            }

            dict.Add(key, value);
        }
    }
}
