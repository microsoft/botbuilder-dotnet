using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Schema;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.LUIS;

namespace Microsoft.Bot.Samples.LUIS
{
    public class LUISBot : IBot
    {

        public Task OnReceiveActivity(IBotContext context)
        {
            if (context.Request.Type == ActivityTypes.Message)
            {

                // Get list of itents
                var results = context.Get<RecognizerResult>("LuisRecognizerResult");

                // Get top Intent
                var topIntent = results.GetTopScoringIntent();

                switch (topIntent.key)
                {
                    case null:
                    case "None":
                        context.SendActivity("Apologies, I dont understand");
                        break;
                    default:
                        context.SendActivity($"Itent: {topIntent.key}, Entities: {results.Entities.Count}");
                        break;
                }
            }

            return Task.CompletedTask;
        }

    }

}