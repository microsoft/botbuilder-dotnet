using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;

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

        public static void AdapterNotNull(ActivityAdapterBase adapter)
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
