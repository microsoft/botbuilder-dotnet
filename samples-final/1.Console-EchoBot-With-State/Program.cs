// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;
using Microsoft.Bot;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Schema;

namespace Console_EchoBot_With_State
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to the EchoBot. Type something to get started.");

            // Create the Console Adapter, and add Conversation State 
            // to the Bot. The Conversation State will be stored in memory. 
            var adapter = new ConsoleAdapter()
                .Use(new ConversationState<EchoState>(new MemoryStorage()));

            // Create the instance of our Bot.
            var echoBot = new EchoBot();

            // Connect the Console Adapter to the Bot. 
            adapter.ProcessActivity(
                async (context) => await echoBot.OnTurn(context)).Wait();
        }
    }

    public class EchoBot : IBot
    {
        /// <summary>
        /// Every Conversation turn for our EchoBot will call this method. In here
        /// the bot checks the Activty type to verify it's a message, bumps the 
        /// turn conversation 'Turn' count, and then echoes the users typing
        /// back to them. 
        /// </summary>
        /// <param name="context">Turn scoped context containing all the data needed
        /// for processing this conversation turn. </param>        
        public async Task OnTurn(ITurnContext context)
        {
            // This bot is only handling Messages
            if (context.Activity.Type == ActivityTypes.Message)
            {
                // Get the conversation state from the turn context
                var state = context.GetConversationState<EchoState>();

                // Bump the turn count. 
                state.TurnCount++;

                // Echo back to the user whatever they typed.
                await context.SendActivity($"Turn {state.TurnCount}: You sent '{context.Activity.Text}'");
            }
        }
    }

    /// <summary>
    /// Class for storing conversation state. 
    /// </summary>
    public class EchoState
    {
        public int TurnCount { get; set; } = 0;
    }
}
