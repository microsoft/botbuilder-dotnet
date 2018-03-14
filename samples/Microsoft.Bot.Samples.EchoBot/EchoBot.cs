// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Schema;
using System.Threading.Tasks;

namespace Microsoft.Bot.Samples.Echo
{
    public class EchoState : StoreItem
    {
        public int TurnNumber { get; set; }
    }

    public class EchoBot : IBot
    {
        private readonly IMyService _myService;

        public EchoBot(IMyService myService)
        {
            _myService = myService;
        }

        public async Task OnReceiveActivity(IBotContext context)
        {            
            if (context.Request.Type == ActivityTypes.Message)
            {
                var conversationState = context.GetConversationState<EchoState>() ?? new EchoState();

                conversationState.TurnNumber++;

                // calculate something for us to return
                int length = (context.Request.Text ?? string.Empty).Length;

                // simulate calling a dependent service that was injected
                await _myService.DoSomethingAsync();

                // return our reply to the user
                string response = $"[{conversationState.TurnNumber}] You sent {context.Request.Text} which was {length} characters";
                await context.SendActivity(response); 
            }
            
            var convUpdateActivity = context.Request.AsConversationUpdateActivity();
            if (convUpdateActivity != null)
            {
                foreach (var newMember in convUpdateActivity.MembersAdded)
                {
                    if (newMember.Id != convUpdateActivity.Recipient.Id)
                    {
                        await context.SendActivity("Hello and welcome to the echo bot.");                         
                    }
                }
            }
        }
    }
}