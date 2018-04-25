using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Samples.Echo.AspNetCore
{
    public class EchoBot : IBot
    {
        public EchoBot() { }

        public async Task OnTurn(ITurnContext context)
        {
            switch (context.Activity.Type)
            {
                // Note: This Sample is soon to be deleted. I've added in some easy testing here
                // for now, although we do need a better solution for testing the BotFrameworkConnector in the
                // near future

                case ActivityTypes.Message:
                    await context.SendActivity($"You sent '{context.Activity.Text}'");
                    if (context.Activity.Text.ToLower() == "getmembers")
                    {
                        BotFrameworkAdapter b = (BotFrameworkAdapter)context.Adapter;
                        var members = await b.GetConversationMembers(context);
                        await context.SendActivity($"Members Found: {members.Count} "); 
                        foreach ( var m in members)
                        {
                            await context.SendActivity($"Member Id: {m.Id} Name: {m.Name} Role: {m.Role}");
                        }
                    }
                    else if (context.Activity.Text.ToLower() == "getactivitymembers")
                    {
                        BotFrameworkAdapter b = (BotFrameworkAdapter)context.Adapter;
                        var members = await b.GetActivityMembers(context);
                        await context.SendActivity($"Members Found: {members.Count} ");
                        foreach (var m in members)
                        {
                            await context.SendActivity($"Member Id: {m.Id} Name: {m.Name} Role: {m.Role}");
                        }
                    }
                    else if (context.Activity.Text.ToLower() == "getconversations")
                    {
                        BotFrameworkAdapter b = (BotFrameworkAdapter)context.Adapter;
                        var conversations = await b.GetConversations(context); 
                        await context.SendActivity($"Conversations Found: {conversations.Conversations.Count} ");
                        foreach (var m in conversations.Conversations)
                        {
                            await context.SendActivity($"Conversation Id: {m.Id} Member Count: {m.Members.Count}");                            
                        }
                    }

                    break;
                case ActivityTypes.ConversationUpdate:
                    foreach (var newMember in context.Activity.MembersAdded)
                    {
                        if (newMember.Id != context.Activity.Recipient.Id)
                        {
                            await context.SendActivity("Hello and welcome to the echo bot.");
                        }
                    }
                    break;
            }
        }
    }
}
