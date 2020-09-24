// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// When added, this middleware will send typing activities back to the user when a Message activity
    /// is received to let them know that the bot has received the message and is working on the response.
    /// You can specify a delay in milliseconds before the first typing activity is sent and then a frequency,
    /// also in milliseconds which determines how often another typing activity is sent. Typing activities
    /// will continue to be sent until your bot sends another message back to the user.
    /// </summary>
    public class ShowTypingMiddleware : IMiddleware
    {
        private readonly TimeSpan _delay;
        private readonly TimeSpan _period;

        /// <summary>
        /// Initializes a new instance of the <see cref="ShowTypingMiddleware"/> class.
        /// </summary>
        /// <param name="delay">Initial delay before sending first typing indicator. Defaults to 500ms.</param>
        /// <param name="period">Rate at which additional typing indicators will be sent. Defaults to every 2000ms.</param>
        public ShowTypingMiddleware(int delay = 500, int period = 2000)
        {
            if (delay < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(delay), "Delay must be greater than or equal to zero");
            }

            if (period <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(period), "Repeat period must be greater than zero");
            }

            _delay = TimeSpan.FromMilliseconds(delay);
            _period = TimeSpan.FromMilliseconds(period);
        }

        /// <summary>
        /// Processes an incoming activity.
        /// </summary>
        /// <param name="turnContext">The context object for this turn.</param>
        /// <param name="next">The delegate to call to continue the bot middleware pipeline.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>Spawns a thread that sends the periodic typing activities until the turn ends.
        /// </remarks>
        /// <seealso cref="ITurnContext"/>
        /// <seealso cref="Bot.Schema.IActivity"/>
        public async Task OnTurnAsync(ITurnContext turnContext, NextDelegate next, CancellationToken cancellationToken)
        {
            using (var cts = new CancellationTokenSource())
            {
                Task typingTask = null;
                try
                {
                    // Start a timer to periodically send the typing activity (bots running as skills should not send typing activity)
                    if (!IsSkillBot(turnContext) && turnContext.Activity.Type == ActivityTypes.Message)
                    {
                        // do not await task - we want this to run in the background and we will cancel it when its done
                        typingTask = SendTypingAsync(turnContext, _delay, _period, cts.Token);
                    }

                    await next(cancellationToken).ConfigureAwait(false);
                }
                finally
                {
                    if (typingTask != null && !typingTask.IsCanceled)
                    {
                        // Cancel the typing loop.
                        cts.Cancel();
                    }
                }
            }
        }

        private static bool IsSkillBot(ITurnContext turnContext)
        {
            return turnContext.TurnState.Get<IIdentity>(BotAdapter.BotIdentityKey) is ClaimsIdentity claimIdentity
                && SkillValidation.IsSkillClaim(claimIdentity.Claims);
        }

        private static async Task SendTypingAsync(ITurnContext turnContext, TimeSpan delay, TimeSpan period, CancellationToken cancellationToken)
        {
            try
            {
                await Task.Delay(delay, cancellationToken).ConfigureAwait(false);

                while (!cancellationToken.IsCancellationRequested)
                {
                    if (!cancellationToken.IsCancellationRequested)
                    {
                        await SendTypingActivityAsync(turnContext, cancellationToken).ConfigureAwait(false);
                    }

                    // if we happen to cancel when in the delay we will get a TaskCanceledException
                    await Task.Delay(period, cancellationToken).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
                // do nothing
            }
        }

        private static async Task SendTypingActivityAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            // create a TypingActivity, associate it with the conversation and send immediately
            var typingActivity = new Activity
            {
                Type = ActivityTypes.Typing,
                RelatesTo = turnContext.Activity.RelatesTo,
            };

            // sending the Activity directly on the Adapter avoids other Middleware and avoids setting the Responded
            // flag, however, this also requires that the conversation reference details are explicitly added.
            var conversationReference = turnContext.Activity.GetConversationReference();
            typingActivity.ApplyConversationReference(conversationReference);

            // make sure to send the Activity directly on the Adapter rather than via the TurnContext
            await turnContext.Adapter.SendActivitiesAsync(turnContext, new Activity[] { typingActivity }, cancellationToken).ConfigureAwait(false);
        }
    }
}
