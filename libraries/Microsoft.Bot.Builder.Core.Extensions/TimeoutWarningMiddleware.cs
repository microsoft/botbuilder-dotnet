using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Core.Extensions
{
    public class TimeoutWarningMiddleware : IMiddleware
    {
        /// <summary>
        /// Period of time to wait before logging a warning to indicate a potential channel timeout before the bot responds. Defaults to 8000ms.
        /// </summary>
        private readonly int _timeoutPeriod;

        public TimeoutWarningMiddleware(int timeoutPeriod = 8000)
        {
            _timeoutPeriod = timeoutPeriod;
        }

        public async Task OnProcessRequest(IBotContext context, MiddlewareSet.NextDelegate next)
        {
            Timer timeoutWarningTimer = new Timer(TimeoutWarningTimerCallback, context, _timeoutPeriod, 0);

            await next().ConfigureAwait(false);

            // Once the bot has processed the request, the middleware should dispose of the timer
            // on the trailing edge of the request
            timeoutWarningTimer?.Dispose();
        }

        private void TimeoutWarningTimerCallback(object state)
        {
            double seconds = (double)_timeoutPeriod / (double)1000;
            var warning = $"WARNING: The bot has taken longer than {seconds} to respond. After 8-12 seconds some channels may timeout causing message failure or re-delivery.";
            Trace.TraceWarning(warning);
        }
    }
}
