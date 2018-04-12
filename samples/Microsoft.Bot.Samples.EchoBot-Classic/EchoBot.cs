using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Samples.Echo.Classic;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Builder.Classic.Dialogs;

namespace Microsoft.Bot.Samples.Echo.AspNetWebApi
{
    public class EchoBot : IBot
    {
        public EchoBot() { }

        public async Task OnTurn(ITurnContext context)
        {
            switch (context.Activity.Type)
            {
                case ActivityTypes.Message:
                    // Use Classic IDialog 
                    await Conversation.SendAsync(context, () => new EchoDialog());
                    break;

                case ActivityTypes.ConversationUpdate:
                    foreach (var newMember in context.Activity.MembersAdded)
                    {
                        if (newMember.Id != context.Activity.Recipient.Id)
                        {
                            await context.SendActivity("Hello and welcome to the echo bot.");
                        }
                    }
                    break;
            }
        }
    }
}