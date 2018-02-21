using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.BotFramework;
using Microsoft.Bot.Schema;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace Microsoft.Bot.Samples.EchoBot_AspNet461
{
    public class MessagesController : ApiController
    {
        private readonly BotFrameworkAdapter _adapter;

        public MessagesController(Builder.Bot bot)
        {
            _adapter = bot.Adapter as BotFrameworkAdapter;

            bot.OnReceive(BotReceiveHandler);
        }

        private Task BotReceiveHandler(IBotContext context)
        {
            var msgActivity = context.Request.AsMessageActivity();
            if (msgActivity != null)
            {
                long turnNumber = context.State.Conversation["turnNumber"] ?? 0;
                context.State.Conversation["turnNumber"] = ++turnNumber;

                // calculate something for us to return
                int length = (msgActivity.Text ?? string.Empty).Length;

                // return our reply to the user
                context.Reply($"[{turnNumber}] You sent {msgActivity.Text} which was {length} characters");
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

        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity) => new HttpResponseMessage((System.Net.HttpStatusCode)(await _adapter.Receive(this.Request.Headers, activity)));
    }
}