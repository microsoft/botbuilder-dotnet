using Microsoft.Bot.Connector;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Adapters
{
    public abstract class ActivityAdapterBase
    {
        public delegate Task OnReceiveDelegate(Activity activity, CancellationToken token);

        public ActivityAdapterBase() { }

        public OnReceiveDelegate OnReceive { get; set; }               

        public abstract Task Post(IList<Activity> activities, CancellationToken token);
    }
}
