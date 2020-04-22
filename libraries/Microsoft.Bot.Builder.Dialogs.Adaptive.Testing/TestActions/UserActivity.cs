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
    /// Send an activity to the bot.
    /// </summary>
    public class UserActivity : TestAction
    {
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.Test.UserActivity";

        [JsonConstructor]
        public UserActivity([CallerFilePath] string path = "", [CallerLineNumber] int line = 0)
        {
            RegisterSourcePath(path, line);
        }

        /// <summary>
        /// Gets or sets the activity to compare.
        /// </summary>
        /// <value>
        /// The activity to compare.
        /// </value>
        [JsonProperty("activity")]
        public Activity Activity { get; set; }

        /// <summary>
        /// Gets or sets the User name.
        /// </summary>
        /// <value>
        /// If user is set then the channalAccount.Id and channelAccount.Name will be from user.
        /// </value>
        [JsonProperty("user")]
        public string User { get; set; }

        public async override Task ExecuteAsync(TestAdapter adapter, BotCallbackHandler callback)
        {
            if (this.Activity == null)
            {
                throw new Exception("You must define one of Text or Activity properties");
            }

            var activity = ObjectPath.Clone(this.Activity);
            activity.ApplyConversationReference(adapter.Conversation, isIncoming: true);

            if (!string.IsNullOrEmpty(this.User))
            {
                activity.From = ObjectPath.Clone(activity.From);
                activity.From.Id = this.User;
                activity.From.Name = this.User;
            }

            Stopwatch sw = new Stopwatch();
            sw.Start();
            
            await adapter.ProcessActivityAsync(this.Activity, callback, default(CancellationToken)).ConfigureAwait(false);
            
            sw.Stop();
            Trace.TraceInformation($"[Turn Ended => {sw.ElapsedMilliseconds} ms processing UserActivity: {this.Activity.Text} ]");
        }
    }
}
