using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AutoGuru.KeyValuePush
{
    public interface IPusher
    {
        Task PushAsync(
            IDictionary<string, string> dictionary, 
            CancellationToken cancellationToken);
    }
}
