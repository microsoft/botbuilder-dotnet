using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.AI.LanguageGeneration;
using System.IO;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.TestBot.Json
{
    public class TestBotLG : IBot
    {
        private DialogSet _dialogs;
        private SemaphoreSlim _semaphore;

        private readonly TemplateEngine engine;

        private string GetLGResourceFile(string fileName)
        {
            return AppContext.BaseDirectory.Substring(0, AppContext.BaseDirectory.IndexOf("bin")) + "LG\\" + fileName;
        }

        public TestBotLG(TestBotAccessors accessors)
        {
            // load LG file into engine
            engine = TemplateEngine.FromFile(GetLGResourceFile("3.LG"));
        }

        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (turnContext.Activity.Type == ActivityTypes.Message)
            {
                if (turnContext.Activity.Text.ToLower() == "hi")
                {
                    await turnContext.SendActivityAsync(engine.EvaluateTemplate("GreetingTemplate", null));
                } else
                {
                    await turnContext.SendActivityAsync(engine.EvaluateTemplate("EchoTemplate", turnContext));
                }
            }
            else if (turnContext.Activity.Type == ActivityTypes.ConversationUpdate)
            {
                if (turnContext.Activity.MembersAdded != null)
                {
                    // Iterate over all new members added to the conversation
                    foreach (var member in turnContext.Activity.MembersAdded)
                    {
                        // Greet anyone that was not the target (recipient) of this message
                        // the 'bot' is the recipient for events from the channel,
                        // turnContext.Activity.MembersAdded == turnContext.Activity.Recipient.Id indicates the
                        // bot was added to the conversation.
                        if (member.Id != turnContext.Activity.Recipient.Id)
                        {
                            await turnContext.SendActivityAsync(engine.EvaluateTemplate("WelcomeTemplate", null));
                        }
                    }
                }
            }
            else 
            {
                await turnContext.SendActivityAsync($"{turnContext.Activity.Type} event detected");
            }
        }
    }
}
