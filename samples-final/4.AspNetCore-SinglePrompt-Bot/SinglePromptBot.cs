// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;

namespace AspNetCore_Single_Prompts
{
    public class SinglePromptBot : IBot
    {
        public async Task OnTurn(ITurnContext context)
        {
            var state = ConversationState<Dictionary<string, object>>.Get(context);
            var prompt = new TextPrompt();
            var options = new PromptOptions { PromptString = "Hello, I'm the demo bot. What is your name?" };

            switch (context.Activity.Type)
            {
                case ActivityTypes.ConversationUpdate:
                    if (context.Activity.MembersAdded[0].Id != context.Activity.Recipient.Id)
                    {
                        await prompt.Begin(context, state, options);
                    }
                    break;
                case ActivityTypes.Message:
                    var dialogCompletion = await prompt.Continue(context, state);
                    if (!dialogCompletion.IsActive && !dialogCompletion.IsCompleted)
                    {
                        await prompt.Begin(context, state, options);
                    }
                    else if (dialogCompletion.IsCompleted)
                    {
                        var textResult = (Microsoft.Bot.Builder.Prompts.TextResult)dialogCompletion.Result;
                        await context.SendActivity($"'{textResult.Value}' is a great name!");
                    }
                    break;
            }
        }
    }    
}
