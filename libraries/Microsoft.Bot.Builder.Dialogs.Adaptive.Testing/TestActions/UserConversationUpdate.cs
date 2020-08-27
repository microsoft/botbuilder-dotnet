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
    /// <summary>
    /// Action to script sending a conversationUpdate activity to the bot.
    /// </summary>
    [DebuggerDisplay("UserConversationUpdate")]
    public class UserConversationUpdate : TestAction
    {
        /// <summary>
        /// Kind for serialization.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.Test.UserConversationUpdate";

        /// <summary>
        /// Initializes a new instance of the <see cref="UserConversationUpdate"/> class.
        /// </summary>
        /// <param name="path">path for source.</param>
        /// <param name="line">line number in source.</param>
        [JsonConstructor]
        public UserConversationUpdate([CallerFilePath] string path = "", [CallerLineNumber] int line = 0)
        {
            RegisterSourcePath(path, line);
        }

        /// <summary>
        /// Gets the members added names.
        /// </summary>
        /// <value>The members names.</value>
        [JsonProperty("membersAdded")]
        public List<string> MembersAdded { get; } = new List<string>();

        /// <summary>
        /// Gets the members removed names.
        /// </summary>
        /// <value>The members names.</value>
        [JsonProperty("membersRemoved")]
        public List<string> MembersRemoved { get; } = new List<string>();

        /// <inheritdoc/>
        public override async Task ExecuteAsync(TestAdapter adapter, BotCallbackHandler callback)
        {
            var activity = adapter.MakeActivity();
            activity.Type = ActivityTypes.ConversationUpdate;
            if (this.MembersAdded.Any())
            {
                activity.MembersAdded = new List<ChannelAccount>();
                foreach (var member in MembersAdded)
                {
                    activity.MembersAdded.Add(new ChannelAccount(id: member, name: member, role: RoleTypes.User));
                }
            }

            if (this.MembersRemoved.Any())
            {
                activity.MembersRemoved = new List<ChannelAccount>();

                foreach (var member in MembersRemoved)
                {
                    activity.MembersRemoved.Add(new ChannelAccount(id: member, name: member, role: RoleTypes.User));
                }
            }

            Stopwatch sw = new Stopwatch();
            sw.Start();

            await adapter.ProcessActivityAsync(activity, callback, default(CancellationToken)).ConfigureAwait(false);

            sw.Stop();
            Trace.TraceInformation($"[Turn Ended => {sw.ElapsedMilliseconds} ms processing UserConversationUpdate[]");
        }
    }
}
