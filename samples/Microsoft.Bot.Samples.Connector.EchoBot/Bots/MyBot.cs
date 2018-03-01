using Microsoft.Bot;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Middleware;
using System.Threading.Tasks;

namespace Connector.EchoBot
{
    public class MyBotState : StoreItem
    {
        public string eTag { get; set; }

        public int TurnNumber { get; set; }
    }

    public class MyBot : IBot
    {
        private readonly IMyService _myService;

        public MyBot(IMyService myService)
        {
            _myService = myService;
        }

        public async Task OnReceiveActivity(IBotContext context)
        {
            var msgActivity = context.Request.AsMessageActivity();
            if (msgActivity != null)
            {
                var conversationState = context.GetConversationState<MyBotState>() ?? new MyBotState();

                conversationState.TurnNumber++;

                // calculate something for us to return
                int length = (msgActivity.Text ?? string.Empty).Length;

                // simulate calling a dependent service that was injected
                await _myService.DoSomethingAsync();

                // return our reply to the user
                context.Reply($"[{conversationState.TurnNumber}] You sent {msgActivity.Text} which was {length} characters");
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
            }
        }
    }

    public interface IMyService
    {
        Task DoSomethingAsync();
    }

    public sealed class MyService : IMyService
    {
        public Task DoSomethingAsync() => Task.Delay(500);
    }
}