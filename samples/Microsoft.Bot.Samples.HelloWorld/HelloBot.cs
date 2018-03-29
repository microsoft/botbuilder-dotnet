using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Samples
{
    public class HelloBot : IBot
    {
        public async Task OnReceiveActivity(IBotContext context)
        {
            if (context.Request.Type is ActivityTypes.Message)
            {
                await context.SendActivity($"Hello world.");
            }

        }
    }
}