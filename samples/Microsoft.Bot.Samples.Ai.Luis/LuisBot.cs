using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.LUIS;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Samples.Ai.Luis
{
    public class LuisBot : IBot
    {
        public LuisBot() { }

        public async Task OnReceiveActivity(ITurnContext context)
        {
            switch (context.Activity.Type)
            {
                case ActivityTypes.Message:

                    var luisResult = context.Get<RecognizerResult>(LuisRecognizerMiddleware.LuisRecognizerResultKey);

                    if (luisResult != null)
                    {
                        (string key, double score) topItem = luisResult.GetTopScoringIntent();
                        await context.SendActivity($"The **top intent** was: **'{topItem.key}'**, with score **{topItem.score}**");

                        await context.SendActivity($"Detail of intents scorings:");
                        var intentsResult = new List<string>();
                        foreach (var intent in luisResult.Intents)
                        {
                            intentsResult.Add($"* '{intent.Key}', score {intent.Value}");
                        }
                        await context.SendActivity(string.Join("\n\n", intentsResult));
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
