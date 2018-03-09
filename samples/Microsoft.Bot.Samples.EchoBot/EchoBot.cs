// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Core.Extensions;
using System.Threading.Tasks;

namespace Microsoft.Bot.Samples.Echo
{
    public class EchoState : StoreItem
    {
        public string eTag { get; set; }

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
            var msgActivity = context.Request.AsMessageActivity();
            if (msgActivity != null)
            {
                var conversationState = context.GetConversationState<EchoState>() ?? new EchoState();

                conversationState.TurnNumber++;

                // calculate something for us to return
                int length = (msgActivity.Text ?? string.Empty).Length;

                // simulate calling a dependent service that was injected
                await _myService.DoSomethingAsync();

                // return our reply to the user
                context.Batch().Reply($"[{conversationState.TurnNumber}] You sent {msgActivity.Text} which was {length} characters");
            }
            
            var convUpdateActivity = context.Request.AsConversationUpdateActivity();
            if (convUpdateActivity != null)
            {
                foreach (var newMember in convUpdateActivity.MembersAdded)
                {
                    if (newMember.Id != convUpdateActivity.Recipient.Id)
                    {
                        context.Batch().Reply("Hello and welcome to the echo bot.");
                    }
                }
            }
        }
    }
}