using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Samples.EchoBot_ASPNetCore_DI.Controllers
{
    [Route("api/[controller]")]
    public class MessagesController : Controller
    {
        private readonly BotAdapterHelper botAdapter;

        public MessagesController(BotAdapterHelper botAdapter)
        {
            this.botAdapter = botAdapter;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Activity activity)
        {
            await this.botAdapter.ProcessActivity(activity, (context) =>
            {
                var msgActivity = context.Request.AsMessageActivity();
                if (msgActivity != null)
                {
                    long turnNumber = context.State.ConversationProperties["turnNumber"] ?? 0;
                    context.State.ConversationProperties["turnNumber"] = ++turnNumber;

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
            });

            return this.Ok();
        }
    }
}
