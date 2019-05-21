using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Xunit.Abstractions;
using Activity = Microsoft.Bot.Schema.Activity;

namespace Microsoft.BotBuilderSamples.Tests.Utils.XUnit
{
    /// <summary>
    /// A middleware to output incoming and outgoing activities as json strings to the console during
    /// unit tests
    /// </summary>
    public class XUnitOutputMiddleware : IMiddleware
    {
        private const string XUnitStopWatchStateKey = "XUnitStopwatch";
        private readonly ITestOutputHelper _output;

        public XUnitOutputMiddleware(ITestOutputHelper xunitOutputHelper)
        {
            _output = xunitOutputHelper;
        }

        public async Task OnTurnAsync(ITurnContext context, NextDelegate next, CancellationToken cancellationToken = default(CancellationToken))
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            context.TurnState[XUnitStopWatchStateKey] = stopwatch;
            LogActivity("User: ", context.Activity);
            context.OnSendActivities(OnSendActivities);

            await next(cancellationToken).ConfigureAwait(false);
        }

        private static string GetTextOrSpeak(IMessageActivity messageActivity) => string.IsNullOrWhiteSpace(messageActivity.Text) ? messageActivity.Speak : messageActivity.Text;

        private async Task<ResourceResponse[]> OnSendActivities(ITurnContext context, List<Activity> activities, Func<Task<ResourceResponse[]>> next)
        {
            var stopwatch = (Stopwatch)context.TurnState[XUnitStopWatchStateKey];
            foreach (var response in activities)
            {
                LogActivity("Bot:  ", response, stopwatch);
            }

            return await next();
        }

        private void LogActivity(string prefix, Activity contextActivity, Stopwatch stopwatch = null)
        {
            _output.WriteLine(string.Empty);
            if (contextActivity.Type == ActivityTypes.Message)
            {
                var messageActivity = contextActivity.AsMessageActivity();
                _output.WriteLine($"{prefix} {GetTextOrSpeak(messageActivity)}");
            }
            else if (contextActivity.Type == ActivityTypes.Event)
            {
                var eventActivity = contextActivity.AsEventActivity();
                _output.WriteLine($"{prefix} Event: {eventActivity.Name}");
            }
            else
            {
                _output.WriteLine($"{prefix} {contextActivity.Type}");
            }

            var timingInfo = $"       -> ts: {DateTime.Now:hh:mm:ss}";
            if (stopwatch != null)
            {
                timingInfo += $" elapsed: {stopwatch.ElapsedMilliseconds:N0} ms";
                stopwatch.Restart();
            }

            _output.WriteLine(timingInfo);
        }
    }
}
