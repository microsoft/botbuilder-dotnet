using Microsoft.Bot.Connector;
using Microsoft.Extensions.Primitives;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder
{
    public interface IConnector
    {
        Task Receive(IActivity activity, CancellationToken token);

        Task Post(IList<IActivity> activities, CancellationToken token);
    }

    public interface IHttpConnector : IConnector
    {
        Task Receive(IDictionary<string, StringValues> headers, IActivity activity, CancellationToken token);
    }
}
