using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Ai.LUIS;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Schema;

namespace AspNetCore_LuisLG_Bot
{
    public class LuisLanguageGenerationBot : IBot
    {
        public async Task OnTurn(ITurnContext context)
        {
            switch (context.Activity.Type)
            {
                case ActivityTypes.Message:
                    var luisResult = context.Services.Get<RecognizerResult>(LuisRecognizerMiddleware.LuisRecognizerResultKey);

                    var (intent, score) = luisResult.GetTopScoringIntent();
                    var outgoingActivity = new Activity();
                    if (intent == "greeting")
                    {
                        outgoingActivity.Text = "hello my friend";
                        await context.SendActivity(outgoingActivity);

                    }
                    else if (intent == "help")
                    {
                        outgoingActivity.Text = "how can I help you dear?";
                        await context.SendActivity(outgoingActivity);

                    }
                    break;
                case ActivityTypes.ConversationUpdate:
                    foreach (var newMember in context.Activity.MembersAdded)
                    {
                        if (newMember.Id != context.Activity.Recipient.Id)
                        {
                            await context.SendActivity("Hello and welcome to the Luis Sample bot.");
                        }
                    }
                    break;
            }
        }
    }
}
