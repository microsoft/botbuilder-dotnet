using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Core.Tests
{
    public class SimpleAdapter : BotAdapter
    {
        private readonly Action<Activity[]> _callOnSend = null;
        private readonly Action<Activity> _callOnUpdate = null;
        private readonly Action<ConversationReference> _callOnDelete = null;

        public SimpleAdapter() { }
        public SimpleAdapter(Action<Activity[]> callOnSend) { _callOnSend = callOnSend; }
        public SimpleAdapter(Action<Activity> callOnUpdate) { _callOnUpdate = callOnUpdate; }
        public SimpleAdapter(Action<ConversationReference> callOnDelete) { _callOnDelete = callOnDelete; }

        public async override Task DeleteActivity(IBotContext context, ConversationReference reference)
        {
            Assert.IsNotNull(reference, "SimpleAdapter.deleteActivity: missing reference");
            _callOnDelete?.Invoke(reference);
        }

        public async override Task SendActivity(IBotContext context, params Activity[] activities)
        {
            Assert.IsNotNull(activities, "SimpleAdapter.deleteActivity: missing reference");
            Assert.IsTrue(activities.Count() > 0, "SimpleAdapter.sendActivities: empty activities array.");

            _callOnSend?.Invoke(activities);
        }

        public async override Task<ResourceResponse> UpdateActivity(IBotContext context, Activity activity)
        {
            Assert.IsNotNull(activity, "SimpleAdapter.updateActivity: missing activity");
            _callOnUpdate?.Invoke(activity);
            return new ResourceResponse("testId");
        }

        public async Task ProcessRequest(Activity activty, Func<IBotContext, Task> callback)
        {
            BotContext ctx = new BotContext(this, activty);
            await this.RunPipeline(ctx, callback); 
        }
    }

}
