using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Samples.Echo.AspNetCore
{
    public class EchoBot : IBot
    {
        public EchoBot() { }

        public async Task OnReceiveActivity(ITurnContext context)
        {
            switch (context.Request.Type)
            {
                case ActivityTypes.Message:
                    await context.SendActivity($"You sent '{context.Request.Text}'");
                    break;
                case ActivityTypes.ConversationUpdate:
                    foreach (var newMember in context.Request.MembersAdded)
                    {
                        if (newMember.Id != context.Request.Recipient.Id)
                        {
                            await context.SendActivity("Hello and welcome to the echo bot.");
                        }
                    }
                    break;
            }
        }
    }
}
