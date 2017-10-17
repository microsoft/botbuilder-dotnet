using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Adapters;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Microsoft.Bot.Builder
{
    public class BotAssert
    {
        public static void ActivityNotNull(IActivity activity)
        {
            if (activity == null)
                throw new ArgumentNullException(nameof(activity));
        }
        public static void ContextNotNull(IBotContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context)); 
        }

        public static void CancellationTokenNotNull(CancellationToken token)
        {
            if (token == null)
                throw new ArgumentNullException(nameof(token)); 
        }

        public static void AdapterNotNull(IActivityAdapter adapter)
        {
            if (adapter == null)
                throw new ArgumentNullException(nameof(adapter)); 
        }

        public static void ActivityListNotNull(IList<Activity> activityList)
        {
            if (activityList == null)
                throw new ArgumentNullException(nameof(activityList)); 
        }
        public static void AssertStorage(BotContext context)
        {
            ContextNotNull(context);

            if (context.Storage == null)
                throw new InvalidOperationException("context.storage not found.");
        }

    }
}
