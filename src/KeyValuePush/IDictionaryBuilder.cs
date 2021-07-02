using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace AutoGuru.KeyValuePush
{
    public interface IDictionaryBuilder
    {
        Task<IDictionary<string, string>> BuildAsync(
            string path,
            string searchPattern,
            SearchOption searchOption,
            bool recurseIntoJsonFiles,
            CancellationToken cancellationToken);
    }
}
