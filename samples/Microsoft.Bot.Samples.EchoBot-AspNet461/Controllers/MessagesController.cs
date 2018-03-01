using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Middleware;

namespace Microsoft.Bot.Samples.EchoBot_AspNet461
{
    public class MyState : StoreItem
    {
        public long TurnNumber { get; set; }
    }

    public class MessagesController : BotController
    {

        public MessagesController(BotFrameworkAdapter adapter) : base(adapter) { }

        protected override Task OnReceiveActivity(IBotContext context)
        {
            var msgActivity = context.Request.AsMessageActivity();
            if (msgActivity != null)
            {
                var myState = context.GetConversationState<MyState>();
                myState.TurnNumber++;

                // calculate something for us to return
                int length = (msgActivity.Text ?? string.Empty).Length;

                // return our reply to the user
                context.Reply($"[{myState.TurnNumber}] You sent {msgActivity.Text} which was {length} characters");
                return Task.CompletedTask;
            }

            var convUpdateActivity = context.Request.AsConversationUpdateActivity();
            if (convUpdateActivity != null)
            {
                foreach (var newMember in convUpdateActivity.MembersAdded)
                {
                    if (newMember.Id != convUpdateActivity.Recipient.Id)
                    {
                        context.Reply("Hello and welcome to the echo bot.");
                    }
                }
                return Task.CompletedTask;
            }

            return Task.CompletedTask;
        }
    }
}