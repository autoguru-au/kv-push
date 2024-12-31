using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace AutoGuru.KeyValuePush.Redis
{
    public class Program
    {
        public static async Task<int> Main(string[] args)
        {
            var services = new ServiceCollection()
                .AddSingleton<IDictionaryBuilder, DefaultDictionaryBuilder>()
                .AddSingleton<IPusher, RedisPusher>()
                .AddSingleton<IExecutor, DefaultExecutor>()
                .BuildServiceProvider();

            var app = new CommandLineApplication<Program>();
            app.Conventions
                .UseDefaultConventions()
                .UseConstructorInjection(services);
            return await app.ExecuteAsync(args);
        }

        [Argument(0, Description = "The path to source data from.")]
        [Required]
        public string Path { get; set; }

        [Argument(1,
            Description =
                "The redis configuration to connect to a redis instance with. " +
                "Should be parseable by `StackExchange.Redis.ConfigurationOptions.Parse`.")]
        [Required]
        public string RedisConfiguration { get; set; }

        [Option("-d|--db", Description = "The redis db to use (if any).")]
        public int? RedisDb { get; set; }

        [Option("-sp|--search-pattern",
            Description =
                "The search string to match against the names of files in path. This parameter " +
                "can contain a combination of valid literal path and wildcard (* and ?) characters, " +
                "but it doesn't support regular expressions. " +
                "The default is: \"*\"")]
        public string SearchPattern { get; set; } = "*";

        [Option("-so|--search-option",
            Description =
                "One of the enumeration values that specifies whether the search operation should " +
                "include all subdirectories or only the current directory. " +
                "The default is: TopDirectoryOnly")]
        public SearchOption SearchOption { get; set; } = SearchOption.TopDirectoryOnly;

        [Option("-rj|--recurse-into-json-files",
            Description =
                "Whether to recurse into json files. If true, json files are considered to have " +
                "key-value pairs in them too (e.g. a top-level object with a single level of kvps) and " +
                "these will be crawled, extracted and pushed individually. " +
                "The default is: false.")]
        public bool RecurseIntoJsonFiles { get; set; } = false;

        private readonly IExecutor _executor;
        private readonly RedisPusher _pusher;

#pragma warning disable CS8618 // Arg properties are always set before use by CommandLineUtils
        public Program(IExecutor executor, IPusher pusher)
        {
            _executor = executor;
            _pusher = (RedisPusher)pusher;
        }
#pragma warning restore CS8618

        public async Task<int> OnExecuteAsync(
            CommandLineApplication app,
            CancellationToken cancellationToken = default)
        {
            _pusher.Configure(
                RedisConfiguration,
                RedisDb);

            return await _executor.ExecuteAsync(
                Path, 
                SearchPattern, 
                SearchOption, 
                RecurseIntoJsonFiles, 
                cancellationToken);
        }
    }
}
