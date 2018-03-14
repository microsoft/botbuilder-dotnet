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

        /// <summary>
        /// Middleware to call when a matching Activity type is received
        /// </summary>
        private readonly IMiddleware _nextMiddleware;

        public ActivityFilterMiddleware(string activityType, ActivityFilterHandler handler)
        {
            if (string.IsNullOrEmpty(activityType))
                throw new ArgumentNullException(nameof(activityType));

            // Activity types can be found in ActivityTypes enum
            _activityType = activityType;
            _activityFilterHandler = handler;
        }

        public ActivityFilterMiddleware(string activityType, IMiddleware nextMiddleware)
        {
            // Activity types can be found in ActivityTypes enum
            _activityType = activityType;
            _nextMiddleware = nextMiddleware ?? throw new ArgumentNullException(nameof(activityType));
        }

        public delegate Task ActivityFilterHandler(IBotContext context, MiddlewareSet.NextDelegate next);

        public async Task OnProcessRequest(IBotContext context, MiddlewareSet.NextDelegate next)
        {
            if (string.Equals(context.Request.Type, _activityType, StringComparison.InvariantCultureIgnoreCase))
            {
                // if the incoming Activity type matches the type of activity we are checking for then
                // invoke our handler or next middleware (whevever has been supplied via constructor)

                if (_activityFilterHandler != null)
                {
                    await _activityFilterHandler.Invoke(context, next).ConfigureAwait(false);
                }
                else
                {
                    await _nextMiddleware.OnProcessRequest(context, next).ConfigureAwait(false);
                }
            }
            else
            {
                // If the incoming Activity is not a match then continue routing
                await next().ConfigureAwait(false);
            }
        }
    }
}
