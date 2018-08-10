using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Samples.Echo.AspNetWebApi
{
    public class EchoBot : IBot
    {
        public EchoBot() { }

        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            switch (turnContext.Activity.Type)
            {
                case ActivityTypes.Message:
                    await turnContext.SendActivityAsync($"You sent '{turnContext.Activity.Text}'");
                    break;
                case ActivityTypes.ConversationUpdate:
                    foreach (var newMember in turnContext.Activity.MembersAdded)
                    {
                        if (newMember.Id != turnContext.Activity.Recipient.Id)
                        {
                            await turnContext.SendActivityAsync("Hello and welcome to the echo bot.");
                        }
                    }
                    break;
            }
        }
    }
}
