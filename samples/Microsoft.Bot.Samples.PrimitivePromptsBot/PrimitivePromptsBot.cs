// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;
using Microsoft.Bot;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Builder.Core.Extensions;
using System;

public class BotConversationState : StoreItem
{
    public string ActivePrompt { get; set; }
    public string Name { get; set; }
    public int? Age { get; set; }
}

namespace PrimitivePromptsBot
{
    public class PrimitivePromptsBot : IBot
    {
        public Task OnReceiveActivity(ITurnContext turnContext)
        {
            if (turnContext.Activity.Type == ActivityTypes.Message)
            {
                var message = turnContext.Activity.Text;
                var state = turnContext.GetConversationState<BotConversationState>();

                if (state.ActivePrompt != null)
                {
                    switch (state.ActivePrompt)
                    {
                        case "namePrompt":
                            state.Name = message;
                            break;
                        case "agePrompt":
                            state.Age = Int32.Parse(message);
                            break;
                    }
                    state.ActivePrompt = null;
                }

                if (state.Name == null)
                {
                    state.ActivePrompt = "namePrompt";
                    return turnContext.SendActivity("What is your name?");
                }

                if (state.Age == null)
                {
                    state.ActivePrompt = "agePrompt";
                    return turnContext.SendActivity("How old are you?");
                }

                return turnContext.SendActivity($"Hello {state.Name}! You are {state.Age} years old.");
            }
            else
            {
                return turnContext.SendActivity($"Received activity of type '{turnContext.Activity.Type}'");
            }
        }
    }
}