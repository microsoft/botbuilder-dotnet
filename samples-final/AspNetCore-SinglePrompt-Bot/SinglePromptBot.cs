using System.Threading.Tasks;
using Microsoft.Bot;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Builder.Prompts;
using Microsoft.Bot.Schema;

namespace AspNetCore_EchoBot_With_State
{
    public class SinglePromptBot : IBot
    {
        private readonly TextPrompt namePrompt;

        public SinglePromptBot()
        {
            namePrompt = new TextPrompt(NameValidator);
        }

        private Task NameValidator(ITurnContext context, TextResult toValidate)
        {
            if (context.Activity.Text.Length <= 2)
            {
                toValidate.Status = PromptStatus.NotRecognized;
            }
            return Task.CompletedTask;
        }
       
        public async Task OnTurn(ITurnContext context)
        {
            var state = context.GetConversationState<SinglePromptState>();
            switch (context.Activity.Type)
            {
                case ActivityTypes.ConversationUpdate:
                    state.Prompt = PrompState.Name;
                    await namePrompt.Prompt(context, "Hello, I'm the demo bot. What is your name?");
                    break;
                case ActivityTypes.Message:
                    switch (state.Prompt)
                    {
                        case PrompState.Default: // If the user isn't in a prompt, ask for their name

                            state.Prompt = PrompState.Name;
                            await namePrompt.Prompt(context, "What is your name?");
                            break;
                        case PrompState.Name: // Attempt to recognize the users response
                            var name = await namePrompt.Recognize(context);
                            if (name.Succeeded())
                            {
                                state.Prompt = PrompState.Default;
                                await context.SendActivity($"{name.Text} is a great name!");
                            }
                            else // The user provided an invalid response, so re-prompt the user
                            {
                                await namePrompt.Prompt(context, "Please provide a name longer than 2 characters.");
                            }
                            break;
                    }
                    break;
            }
        }
    }    
}
