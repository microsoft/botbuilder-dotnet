using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Core.Extensions
{
    /// <summary>
    /// When added, this middleware will send typing activities back to the user when a Message activity
    /// is receieved to let them know that the bot has receieved the message and is working on the response.
    /// You can specify a delay in milliseconds before the first typing activity is sent and then a frequency, 
    /// also in milliseconds which determines how often another typing activity is sent. Typing activities 
    /// will continue to be sent until your bot sends another message back to the user.
    /// </summary>
    public class ShowTypingMiddleware : IMiddleware
    {
        /// <summary>
        /// (Optional) initial delay before sending first typing indicator. Defaults to 500ms.
        /// </summary>
        private readonly int _delay;

        /// <summary>
        /// (Optional) rate at which additional typing indicators will be sent. Defaults to every 2000ms.
        /// </summary>
        private readonly int _freqency;

        public ShowTypingMiddleware(int delay = 500, int frequency = 2000)
        {
            if(delay < 0)
                throw new ArgumentOutOfRangeException(nameof(delay), "Delay must be greater than or equal to zero");

            if (frequency <= 0)
                throw new ArgumentOutOfRangeException(nameof(frequency), "Frequency must be greater than zero");

            _delay = delay;
            _freqency = frequency;
        }

        public async Task OnProcessRequest(ITurnContext context, MiddlewareSet.NextDelegate next)
        {
            Timer typingActivityTimer = null;

            try
            {
                // If the incoming activity is a MessageActivity, start a timer to periodically send the typing activity
                if (context.Activity.Type == ActivityTypes.Message)
                {
                    typingActivityTimer = new Timer(SendTypingTimerCallback, context, _delay, _freqency);
                }

                await next().ConfigureAwait(false);
            }
            finally
            {
                // Once the bot has processed the activity, the middleware should dispose of the timer
                // on the trailing edge of the activity.
                typingActivityTimer?.Dispose();
            }
        }

        private async void SendTypingTimerCallback(object state)
        {
            var context = (ITurnContext) state;
            await SendTypingActivity(context);
        }

        private async Task SendTypingActivity(ITurnContext context)
        {
            // create a TypingActivity, associate it with the conversation 
            // and send immediately
            var typingActivity = new Activity
            {
                Type = ActivityTypes.Typing,
                RelatesTo = context.Activity.RelatesTo
            };
            await context.SendActivity(typingActivity);
        }
    }
}
