using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Ai.LUIS;
using Microsoft.Bot.Builder.AI.LanguageGeneration.API;
using Microsoft.Bot.Builder.AI.LanguageGeneration.Resolver;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Schema;

namespace LGLuisSample_Upgraded
{
    public class LuisLanguageGenerationBot : Microsoft.Bot.Builder.IBot
    {
        private LanguageGenerationResolver _languageGenerationResolver;
        public LuisLanguageGenerationBot()
        {
            var endpointKey = "cc7bbcc0-3715-44f0-b7c9-d8fee333dce1";
            var lgAppId = "ab48996d-abe2-4785-8eff-f18d15fc3560";
            var azureRegion = "westus";

            var lgApp = new LanguageGenerationApplication(lgAppId, endpointKey, azureRegion);
            var lgOptions = new LanguageGenerationOptions();
            var resolutionsDictionary = new Dictionary<string, string>
            {
                { "wPhrase", "Hello" },
                { "welcomeUser", "welcome {userName}" },
                { "offerHelp", "How can I help you {userName}?" },
                { "errorReadout", "Sorry, something went wrong, could you repeate this again?" },
            };

            var serviceAgentMock = new ServiceAgentMock(resolutionsDictionary);
            _languageGenerationResolver = new LanguageGenerationResolver(lgApp, lgOptions, serviceAgentMock);
        }
        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            switch (turnContext.Activity.Type)
            {
                case ActivityTypes.Message:
                    var luisResult = turnContext.TurnState.Get<RecognizerResult>(LuisRecognizerMiddleware.LuisRecognizerResultKey);
                    //var userState = turnContext.TurnState.Get<BotState>()
                    //var luisResult = await _luisRecognizer.RecognizeAsync(turnContext, cancellationToken).ConfigureAwait(false);

                    var (intent, score) = luisResult.GetTopScoringIntent();
                    var luisEntities = luisResult.Entities;
                    var outgoingActivity = new Activity();
                    if (intent == "greeting")
                    {
                        outgoingActivity.Text = "[wPhrase] my friend";
                        await _languageGenerationResolver.ResolveAsync(outgoingActivity, new Dictionary<string, object>()).ConfigureAwait(false);
                        await turnContext.SendActivityAsync(outgoingActivity);

                    }
                    else if (intent == "help")
                    {
                        outgoingActivity.Text = "[offerHelp]";
                        object currentUserName = "";
                        turnContext.TurnState.TryGetValue("userName", out currentUserName);
                        var entities = new Dictionary<string, object>()
                        {
                            //{"userName", "Sehemy"}
                            {"userName", (string)currentUserName}
                        };
                        await _languageGenerationResolver.ResolveAsync(outgoingActivity, entities).ConfigureAwait(false);
                        await turnContext.SendActivityAsync(outgoingActivity);

                    }

                    else if (intent == "introduction")
                    {
                        var userName = luisEntities.GetValue("userName")[0].ToString();
                        outgoingActivity.Text = "[wPhrase] " + userName;
                        var entities = new Dictionary<string, object>()
                        {
                            {"userName", userName}
                        };
                        turnContext.TurnState.Add("userName", userName);
                        await _languageGenerationResolver.ResolveAsync(outgoingActivity, entities).ConfigureAwait(false);
                        await turnContext.SendActivityAsync(outgoingActivity);

                    }
                    break;
                case ActivityTypes.ConversationUpdate:
                    foreach (var newMember in turnContext.Activity.MembersAdded)
                    {
                        if (newMember.Id != turnContext.Activity.Recipient.Id)
                        {
                            await turnContext.SendActivityAsync("Hello and welcome to the Luis Sample bot.");
                        }
                    }
                    break;
            }
        }
    }
}
