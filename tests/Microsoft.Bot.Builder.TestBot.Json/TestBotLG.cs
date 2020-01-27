using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.LanguageGeneration;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.TestBot.Json
{
    public class TestBotLG : IBot
    {
        private readonly LGFile lgFile;

        public TestBotLG(TestBotAccessors accessors)
        {
            lgFile = LGParser.ParseFile(GetLGResourceFile("8.LG"));
        }

        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (turnContext.Activity.Type == ActivityTypes.Message)
            {
                if (turnContext.Activity.Text.ToLower() == "hi")
                {
                    await turnContext.SendActivityAsync(lgFile.EvaluateTemplate("GreetingTemplate", null).ToString());
                }
                else if (turnContext.Activity.Text.ToLower().Contains("marco"))
                {
                    await turnContext.SendActivityAsync(lgFile.EvaluateTemplate("WordGameReply", new { GameName = "MarcoPolo" }).ToString());
                }
                else if (turnContext.Activity.Text.ToLower().Contains("what time is it"))
                {
                    await turnContext.SendActivityAsync(lgFile.EvaluateTemplate("TimeOfDayExmple", new { timeOfDay = "morning" }).ToString());
                }
                else if (turnContext.Activity.Text.ToLower().Contains("multi"))
                {
                    await turnContext.SendActivityAsync(lgFile.EvaluateTemplate("MultiLineExample", null).ToString());
                }
                else if (turnContext.Activity.Text.ToLower().Contains("card"))
                {
                    HeroCard card = JsonConvert.DeserializeObject<HeroCard>(lgFile.EvaluateTemplate("CardExample", null).ToString());
                    var reply = turnContext.Activity.CreateReply();
                    reply.Attachments = new List<Attachment>();
                    reply.Attachments.Add(card.ToAttachment());
                    await turnContext.SendActivityAsync(reply);
                }
                else if (turnContext.Activity.Text.ToLower().Contains("weather"))
                {
                    var temp = new
                    {
                        partOfDay = "morning",
                        isAGoodDay = "true",
                        high = "75",
                        low = "33"
                    };

                    await turnContext.SendActivityAsync(lgFile.EvaluateTemplate("WeatherForecast", temp).ToString());
                }
                else
                {
                    await turnContext.SendActivityAsync(lgFile.EvaluateTemplate("EchoTemplate", turnContext).ToString());
                }
            }
            else if (turnContext.Activity.Type == ActivityTypes.ConversationUpdate)
            {
                if (turnContext.Activity.MembersAdded != null)
                {
                    foreach (var member in turnContext.Activity.MembersAdded)
                    {
                        if (member.Id != turnContext.Activity.Recipient.Id)
                        {
                            await turnContext.SendActivityAsync(lgFile.EvaluateTemplate("WelcomeTemplate", null).ToString());
                        }
                    }
                }
            }
            else
            {
                await turnContext.SendActivityAsync($"{turnContext.Activity.Type} event detected");
            }
        }

        private string GetLGResourceFile(string fileName)
        {
            return PathUtils.NormalizePath(AppContext.BaseDirectory.Substring(0, AppContext.BaseDirectory.IndexOf("bin")) + "LG\\" + fileName);
        }
    }
}
