using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Tests
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

        public async override Task DeleteActivity(ITurnContext context, ConversationReference reference)
        {
            Assert.IsNotNull(reference, "SimpleAdapter.deleteActivity: missing reference");
            _callOnDelete?.Invoke(reference);
        }

        public async override Task<ResourceResponse[]> SendActivities(ITurnContext context, Activity[] activities)
        {
            Assert.IsNotNull(activities, "SimpleAdapter.deleteActivity: missing reference");
            Assert.IsTrue(activities.Count() > 0, "SimpleAdapter.sendActivities: empty activities array.");

            _callOnSend?.Invoke(activities);
            List<ResourceResponse> responses = new List<ResourceResponse>();
            foreach(var activity in activities)
            {
                responses.Add(new ResourceResponse(activity.Id));
            }

            return responses.ToArray();
        }

        public async override Task<ResourceResponse> UpdateActivity(ITurnContext context, Activity activity)
        {
            Assert.IsNotNull(activity, "SimpleAdapter.updateActivity: missing activity");
            _callOnUpdate?.Invoke(activity);
            return new ResourceResponse(activity.Id); // echo back the Id
        }

        public async Task ProcessRequest(Activity activty, Func<ITurnContext, Task> callback)
        {
            using (TurnContext ctx = new TurnContext(this, activty))
            {
                await this.RunPipeline(ctx, callback);
            }
        }
    }

}
