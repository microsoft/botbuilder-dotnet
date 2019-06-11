// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Xunit.Abstractions;

namespace Microsoft.Bot.Builder.Testing.XUnit
{
    /// <inheritdoc />
    /// <summary>
    /// A middleware to output incoming and outgoing activities as json strings to the console during
    /// unit tests.
    /// </summary>
    public class XUnitOutputMiddleware : IMiddleware
    {
        private readonly ITestOutputHelper _output;
        private readonly string _stopWatchStateKey;

        /// <summary>
        /// Initializes a new instance of the <see cref="XUnitOutputMiddleware"/> class.
        /// </summary>
        /// <remarks>
        /// This middleware outputs the incoming and outgoing activities for the XUnit based test to the console window.
        /// If you need to output the incoming and outgoing activities to some other provider consider using
        /// the <see cref="TranscriptLoggerMiddleware"/> instead.
        /// </remarks>
        /// <param name="xunitOutputHelper">
        /// An XUnit <see cref="ITestOutputHelper"/> instance.
        /// See <see href="https://xunit.net/docs/capturing-output.html">Capturing Output</see> in the XUnit documentation for additional details.
        /// </param>
        public XUnitOutputMiddleware(ITestOutputHelper xunitOutputHelper)
        {
            _stopWatchStateKey = $"{nameof(XUnitOutputMiddleware)}.Stopwatch.{Guid.NewGuid()}";
            _output = xunitOutputHelper;
        }

        public async Task OnTurnAsync(ITurnContext context, NextDelegate next, CancellationToken cancellationToken = default)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            context.TurnState[_stopWatchStateKey] = stopwatch;
            await LogIncomingActivityAsync(context, context.Activity, cancellationToken).ConfigureAwait(false);
            context.OnSendActivities(OnSendActivitiesAsync);

            await next(cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Logs messages sent from the user to the bot.
        /// </summary>
        /// <remarks>
        /// <see cref="ActivityTypes.Message"/> activities will be logged as text. Other activities will be logged as json.
        /// </remarks>
        /// <param name="context">TODO.</param>
        /// <param name="activity">TODO 1.</param>
        /// <param name="cancellationToken">TODO 2.</param>
        /// <returns>TODO 3.</returns>
        protected virtual Task LogIncomingActivityAsync(ITurnContext context, Activity activity, CancellationToken cancellationToken = default)
        {
            var actor = "User: ";
            if (activity.Type == ActivityTypes.Message)
            {
                var messageActivity = activity.AsMessageActivity();
                _output.WriteLine($"\r\n{actor} {messageActivity.Text}");
            }
            else
            {
                LogActivityAsJson(actor, activity);
            }

            _output.WriteLine($"       -> ts: {DateTime.Now:hh:mm:ss}");
            return Task.FromResult(Task.CompletedTask);
        }

        protected virtual Task LogOutgoingActivityAsync(ITurnContext context, Activity activity, CancellationToken cancellationToken = default)
        {
            var stopwatch = (Stopwatch)context.TurnState[_stopWatchStateKey];
            var actor = "Bot:  ";
            if (activity.Type == ActivityTypes.Message)
            {
                var messageActivity = activity.AsMessageActivity();
                _output.WriteLine($"\r\n{actor} Text={messageActivity.Text}\r\n       Speak={messageActivity.Speak}\r\n       InputHint={messageActivity.InputHint}");
            }
            else
            {
                LogActivityAsJson(actor, activity);
            }

            var timingInfo = $"       -> ts: {DateTime.Now:hh:mm:ss} elapsed: {stopwatch.ElapsedMilliseconds:N0} ms";
            stopwatch.Restart();

            _output.WriteLine(timingInfo);
            return Task.FromResult(Task.CompletedTask);
        }

        private async Task<ResourceResponse[]> OnSendActivitiesAsync(ITurnContext context, List<Activity> activities, Func<Task<ResourceResponse[]>> next)
        {
            foreach (var response in activities)
            {
                await LogOutgoingActivityAsync(context, response, CancellationToken.None).ConfigureAwait(false);
            }

            return await next().ConfigureAwait(false);
        }

        private void LogActivityAsJson(string actor, Activity activity)
        {
            _output.WriteLine($"\r\n{actor} Activity = ActivityTypes.{activity.Type}");
            var s = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
            };
            _output.WriteLine(JsonConvert.SerializeObject(activity, s));
        }
    }
}
