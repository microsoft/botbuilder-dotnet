using Microsoft.Bot.Connector;
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
                throw new ArgumentNullException("activity");
        }
        public static void ContextNotNull(BotContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");
        }

        public static void CancellationTokenNotNull(CancellationToken token)
        {
            if (token == null)
                throw new ArgumentNullException("token");
        }

        public static void ActivityListNotNull(IList<IActivity> activityList)
        {
            if (activityList == null)
                throw new ArgumentNullException("activityList");
        }

    }
}
