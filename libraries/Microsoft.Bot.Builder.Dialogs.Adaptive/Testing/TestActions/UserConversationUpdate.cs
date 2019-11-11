// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Testing.TestActions
{
    public class UserConversationUpdate : TestAction
    {
        public UserConversationUpdate()
        {
        }

        public List<string> MembersAdded { get; set; } = new List<string>();

        public List<string> MembersRemoved { get; set; } = new List<string>();

        public async override Task ExecuteAsync(TestAdapter adapter, BotCallbackHandler callback)
        {
            var activity = adapter.MakeActivity();
            activity.Type = ActivityTypes.ConversationUpdate;
            activity.MembersAdded = new List<ChannelAccount>();
            activity.MembersRemoved = new List<ChannelAccount>();
            foreach (var member in MembersAdded)
            {
                activity.MembersAdded.Add(new ChannelAccount(id: member, name: member, role: RoleTypes.User));
            }

            foreach (var member in MembersRemoved)
            {
                activity.MembersRemoved.Add(new ChannelAccount(id: member, name: member, role: RoleTypes.User));
            }

            await adapter.ProcessActivityAsync(activity, callback, default(CancellationToken)).ConfigureAwait(false);
        }
    }
}
