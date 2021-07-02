using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace AutoGuru.KeyValuePush
{
    public interface IExecutor
    {
        Task<int> ExecuteAsync(
            string path,
            string redisConfiguration,
            int? redisDb,
            string searchPattern,
            SearchOption searchOption,
            bool recurseIntoJsonFiles,
            CancellationToken cancellationToken = default);
    }
}
