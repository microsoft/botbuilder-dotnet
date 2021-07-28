using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using Xunit;
using Activity = Microsoft.Bot.Schema.Activity;

namespace Microsoft.Bot.Builder.Integration.AspNet.Core.Tests
{
    public static class DelayHelper
    {
        public static async Task Test(BotAdapter adapter)
        {
            using var turnContext = new TurnContext(adapter, new Activity());

            var activities = new[]
            {
                new Activity(ActivityTypes.Delay, value: 275),
                new Activity(ActivityTypes.Delay, value: 275L),
                new Activity(ActivityTypes.Delay, value: 275F),
                new Activity(ActivityTypes.Delay, value: 275D),
            };

            Stopwatch sw = new Stopwatch();

            sw.Start();

            await adapter.SendActivitiesAsync(turnContext, activities, default);

            sw.Stop();

            Assert.True(sw.Elapsed.TotalSeconds > 1, $"Delay only lasted {sw.Elapsed}");
        }
    }
}
