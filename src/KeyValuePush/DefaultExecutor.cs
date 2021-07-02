using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace AutoGuru.KeyValuePush
{
    public class DefaultExecutor : IExecutor
    {
        private readonly IDictionaryBuilder _dictionaryBuilder;
        private readonly IPusher _pusher;

        public DefaultExecutor(IDictionaryBuilder dictionaryBuilder, IPusher pusher)
        {
            _dictionaryBuilder = dictionaryBuilder;
            _pusher = pusher;
        }

        public async Task<int> ExecuteAsync(
            string path,
            string searchPattern,
            SearchOption searchOption,
            bool recurseIntoJsonFiles,
            CancellationToken cancellationToken = default)
        {
            IDictionary<string, string> dict;
            try
            {
                dict = await _dictionaryBuilder.BuildAsync(
                    path, 
                    searchPattern, 
                    searchOption, 
                    recurseIntoJsonFiles, 
                    cancellationToken);
            }
            catch (Exception ex)
            {
                WriteException("Unexpected error!", ex);
                return WriteError(ErrorCode.BuilderError);
            }

            try
            {
                await _pusher.PushAsync(
                    dict, 
                    cancellationToken);
            }
            catch (Exception ex)
            {
                WriteException("Unexpected error!", ex);
                return WriteError(ErrorCode.PusherError);
            }

            Console.WriteLine($"Done. {dict.Count} were pushed.");
            return 0;
        }

        private static void WriteException(string msg, Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine(msg);
            Console.Error.WriteLine($"Exception: {ex}");
            Console.ResetColor();
        }

        private static int WriteError(ErrorCode errorCode)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine(Errors.ErrorMessages[errorCode]);
            Console.ResetColor();
            return (int)errorCode;
        }

        public enum ErrorCode
        {
            Unknown = 1,
            BuilderError = 2,
            PusherError = 3
        }

        public static class Errors
        {
            public static readonly Dictionary<ErrorCode, string> ErrorMessages = new()
            {
                { ErrorCode.Unknown, "An unknown error occurred." },
                { ErrorCode.BuilderError, "An error occurred building a dictionary of kvps." },
                { ErrorCode.PusherError, "An error occurred pushing the dictionary of kvps." }
            };
        }
    }
}
