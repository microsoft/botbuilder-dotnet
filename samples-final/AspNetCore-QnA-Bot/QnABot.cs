using System.Threading.Tasks;
using Microsoft.Bot;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace AspNetCore_QnA_Bot
{
    public class QnABot : IBot
    {        
        public async Task OnTurn(ITurnContext context)
        {
            if (context.Activity.Type == ActivityTypes.Message && !context.Responded)
            {
                await context.SendActivity("No QnA Maker answers were found. This example uses a QnA Maker Knowledge Base that focuses on smart light bulbs. To see QnA Maker in action, ask the bot questions like \"Why won't it turn on?\" or say something like \"I need help.\"");
            }
        }
    }    
}
