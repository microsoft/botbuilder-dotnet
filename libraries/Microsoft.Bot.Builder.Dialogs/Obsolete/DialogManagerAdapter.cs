using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Dialogs
{
    [Obsolete("This class is not used anymore", error: true)]
    public class DialogManagerAdapter : BotAdapter
    {
        public DialogManagerAdapter()
        {
        }

        public List<Activity> Activities { get; private set; } = new List<Activity>();

        public override Task<ResourceResponse[]> SendActivitiesAsync(ITurnContext turnContext, Activity[] activities, CancellationToken cancellationToken)
        {
            this.Activities.AddRange(activities);
            return Task.FromResult(activities.Select(a => new ResourceResponse(a.Id)).ToArray());
        }

        public override Task<ResourceResponse> UpdateActivityAsync(ITurnContext turnContext, Activity activity, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public override Task DeleteActivityAsync(ITurnContext turnContext, ConversationReference reference, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
