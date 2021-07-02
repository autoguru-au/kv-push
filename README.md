# kv-push

A .NET global tool/s that can push key-value data to another location.

## How it works

Iterates files in the given path, building a dictionary where keys are
file names (without extensions) and values are the file's contents.

> Note: You can optionally instruct JSON files be recursed for KVs themselves.
They should contain an object of string properties and each will be added to
the dictionary being built.

Finally, the dictionary is pushed to its final destination depending
on what implementation you use.


## Implementations

### kv-push-redis

**Installation**
```bash
> dotnet tool install AutoGuru.KeyValuePush.Redis -g
```

**Usage**
```bash
> kv-push-redis [options] <Path> <RedisConfiguration>
```

```
Arguments:
  Path                           The path to source data from.
  RedisConfiguration             The redis configuration to connect to a redis instance with.

Options:
  -d|--db                        The redis db to use (if any).
  -sp|--search-pattern           The search string to match against the names of files in path. This parameter can
                                 contain a combination of valid literal path and wildcard (* and ?) characters, but it
                                 doesn't support regular expressions. The default is: "*"
  -so|--search-option            One of the enumeration values that specifies whether the search operation should
                                 include all subdirectories or only the current directory. The default is:
                                 TopDirectoryOnly
  -rj|--recurse-into-json-files  Whether to recurse into json files. If true, json files are considered to have
                                 key-value pairs in them too (e.g. a top-level object with a single level of kvps) and
                                 these will be crawled, extracted and pushed individually. The default is: false.
  -?|-h|--help                   Show help information.
```
