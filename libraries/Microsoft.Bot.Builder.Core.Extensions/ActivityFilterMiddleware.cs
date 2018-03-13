using System;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Core.Extensions
{
    public class ActivityFilterMiddleware : IMiddleware
    {
        /// <summary>
        /// The type of Activity that the middleware should check for. e.g. ConversationUpdated or Message
        /// </summary>
        private readonly string _activityType;

        /// <summary>
        /// Handler to call when a matching Activity type is received
        /// </summary>
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
                // if the incoming Activity type matches the type of activity we are checking for then
                // invoke our handler
                await _activityFilterHandler.Invoke(context, next);
            }
            else
            {
                // If the incoming Activity is not a match then continue routing
                await next().ConfigureAwait(false);
            }
        }
    }
}
