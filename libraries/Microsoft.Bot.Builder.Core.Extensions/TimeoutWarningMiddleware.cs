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
            using (new Timer(TimeoutWarningTimerCallback, context, _timeoutPeriod, 0))
            {
                await next().ConfigureAwait(false);
            }
        }

        private void TimeoutWarningTimerCallback(object state)
        {
            var seconds = (double)_timeoutPeriod / (double)1000;
            var warning = $"WARNING: The bot has taken longer than {seconds:F1} to respond. After 8-12 seconds some channels may timeout causing message failure or re-delivery.";
            Trace.TraceWarning(warning);
        }
    }
}
