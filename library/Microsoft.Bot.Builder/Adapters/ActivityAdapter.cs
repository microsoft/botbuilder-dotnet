using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;

namespace Microsoft.Bot.Builder.Adapters
{
    public abstract class ActivityAdapter : IActivityAdapter
    {        
        public Bot Bot {get; set;}        

        public ActivityAdapter()
        {            
        }

        public abstract Task Post(IList<Activity> activities, CancellationToken token);

        public virtual async Task Receive(Activity activity, CancellationToken token)
        {
            BotAssert.ActivityNotNull(activity);
            BotAssert.CancellationTokenNotNull(token);

            await Bot.RunPipeline(activity, token).ConfigureAwait(false) ;
        }
    }

}
