using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Connector;
using Microsoft.Extensions.Primitives;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder
{
    public interface IActivityAdapter
    {
        Task Receive(Activity activity, CancellationToken token);

        Task Post(IList<Activity> activities, CancellationToken token);

        Bot Bot { get; set; }
    }

    public interface IHttpActivityAdapter : IActivityAdapter
    {
        Task Receive(IDictionary<string, StringValues> headers, Activity activity, CancellationToken token);
    }
}
