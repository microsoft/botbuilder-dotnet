using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Ai.LUIS;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Samples.Ai.Luis
{
    public class LuisBot : IBot
    {
        public LuisBot() { }

        public async Task OnTurn(ITurnContext context)
        {
            switch (context.Activity.Type)
            {
                case ActivityTypes.Message:

                    var luisResult = context.Services.Get<RecognizerResult>(LuisRecognizerMiddleware.LuisRecognizerResultKey);

                    if (luisResult != null)
                    {
                        (string topIntent, double score) = luisResult.GetTopScoringIntent();
                        await context.SendActivity($"The **top intent** was: **'{topIntent}'**, with score **{score}**");

                        await context.SendActivity($"Detail of intents scorings:");
                        var intentsResult = new List<string>();
                        foreach (var intent in luisResult.Intents)
                        {
                            var intentScore = (double)intent.Value["score"];
                            intentsResult.Add($"* '{intent.Key}', score {intentScore}");
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
