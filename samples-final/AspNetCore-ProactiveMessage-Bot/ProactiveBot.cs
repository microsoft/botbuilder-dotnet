using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace AspNetCore_ProactiveMessage_Bot
{
    public class ProactiveBot : IBot
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
            if (context.Activity.Type == ActivityTypes.ConversationUpdate &&
                context.Activity.MembersAdded[0].Name == "Bot")
            {
                await context.SendActivity("Hello and welcome to the proactive message bot.");

                // Extract data from the user's message that the bot will need later to send an ad hoc message to the user. 
                ProactiveMessage.FromActivity(context.Activity, "Hello again, this is a proactive message!");


                // Schedule a new proactive message every 10 seconds
                timer = new Timer(
                       o =>
                       {
                           if (!string.IsNullOrEmpty(ProactiveMessage.fromId))
                           {
                               ProactiveMessage.Resume().Wait();
                           }
                       },
                       new object(),
                       TimeSpan.FromSeconds(10),
                       TimeSpan.FromSeconds(10));
            }
        }

        private static Timer timer;
    }
}