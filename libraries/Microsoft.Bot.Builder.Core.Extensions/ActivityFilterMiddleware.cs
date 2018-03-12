using System;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Core.Extensions
{
    public class ActivityFilterMiddleware : IMiddleware
    {
        private readonly string _activityType;
        private readonly ActivityFilterHandler _activityFilterHandler;

        public ActivityFilterMiddleware(string activityType, ActivityFilterHandler handler)
        {
            if (string.IsNullOrEmpty(activityType))
                throw new ArgumentNullException(nameof(activityType));

            _activityType = activityType;
            _activityFilterHandler = handler;
        }

        public delegate Task ActivityFilterHandler(IBotContext context, MiddlewareSet.NextDelegate next);

        public async Task OnProcessRequest(IBotContext context, MiddlewareSet.NextDelegate next)
        {
            if (string.Equals(context.Request.Type, _activityType, StringComparison.InvariantCultureIgnoreCase))
            {
                await _activityFilterHandler.Invoke(context, next);
            }
            else
            {
                await next().ConfigureAwait(false);
            }
        }
    }
}
