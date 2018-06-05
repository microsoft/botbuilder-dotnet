// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;
using Microsoft.Bot;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Builder.Prompts;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text;

namespace AspNetCore_SimplePrompt_Bot
{
    public class SimplePromptBot : IBot
    {
        private readonly TextPrompt namePrompt;
        private readonly NumberPrompt<int> agePrompt;

        public SimplePromptBot()
        {
            namePrompt = new TextPrompt();
            agePrompt = new NumberPrompt<int>(Culture.English, ValidateAge);
        }

        public async Task OnTurn(ITurnContext context)
        {
            if (context.Activity.Type == ActivityTypes.Message)
            {
                var state = context.GetConversationState<SimplePromptState>();

                // Name prompt
                if (!state.PromptinName && !state.PromptinAge)
                {
                    // Prompt for Name
                    state.PromptinName = true;
                    await namePrompt.Prompt(context, "Hello! What is your name?");
                }
                else if (state.PromptinName)
                {
                    // Attempt to recognize the user name
                    var name = await namePrompt.Recognize(context);
                    if (!name.Succeeded())
                    {
                        // Not recognized, re-prompt
                        await namePrompt.Prompt(context, "Sorry, I didn't get that. What is your name?");
                    }
                    else
                    {
                        // Save name and set next state
                        state.Name = name.Value;
                        state.PromptinName = false;
                    }
                }

                // Age Prompt
                if (!string.IsNullOrEmpty(state.Name) && state.Age == 0)
                {
                    // Prompt for age
                    if (!state.PromptinAge)
                    {
                        state.PromptinAge = true;
                        await agePrompt.Prompt(context, $"How old are you, {state.Name}?");
                    }
                    else
                    {
                        var age = await agePrompt.Recognize(context);
                        if (!age.Succeeded())
                        {
                            // Not recognized, re-prompt
                            await agePrompt.Prompt(context, "Sorry, that doesn't look right. Ages 13 to 90 only. What is your age?");
                        }
                        else
                        {
                            // Save age and continue
                            state.Age = age.Value;
                            state.PromptinAge = false;
                        }
                    }
                }

                // Display provided information (if complete)
                if (!string.IsNullOrEmpty(state.Name) && state.Age != 0)
                {
                    await context.SendActivity($"Hello {state.Name}, You are {state.Age}.");

                    // Reset sample by clearing state
                    state.Name = null;
                    state.Age = 0;
                }
            }
        }

        private Task ValidateAge(ITurnContext context, NumberResult<int> result)
        {
            if (result.Value < 13)
            {
                result.Status = PromptStatus.TooSmall;
            }
            else if (result.Value > 90)
            {
                result.Status = PromptStatus.TooBig;
            }
            return Task.CompletedTask;
        }
    }
}
