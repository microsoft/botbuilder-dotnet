// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Dialogs.Debugging;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Testing.TestActions
{
    [DebuggerDisplay("UserConversationUpdate: Added:{string.Join(\",\", MembersAdded)} Removed:{string.Join(\",\", MembersRemoved)}")]
    public class UserConversationUpdate : TestAction
    {
        [JsonProperty("$type")]
        public const string DeclarativeType = "Microsoft.Test.UserConversationUpdate";

        [JsonConstructor]
        public UserConversationUpdate([CallerFilePath] string path = "", [CallerLineNumber] int line = 0)
        {
            RegisterSourcePath(path, line);
        }

        /// <summary>
        /// Gets or sets the members added names.
        /// </summary>
        /// <value>The members names.</value>
        [JsonProperty("membersAdded")]
        public List<string> MembersAdded { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the members removed names.
        /// </summary>
        /// <value>The members names.</value>
        [JsonProperty("membersRemoved")]
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
