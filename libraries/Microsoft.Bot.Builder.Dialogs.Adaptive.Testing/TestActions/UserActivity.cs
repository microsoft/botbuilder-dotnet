// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Testing.TestActions
{
    /// <summary>
    /// Send an activity to the bot.
    /// </summary>
    [DebuggerDisplay("UserActivity")]
    public class UserActivity : TestAction
    {
        /// <summary>
        /// Kind for the serialization.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.Test.UserActivity";

        /// <summary>
        /// Initializes a new instance of the <see cref="UserActivity"/> class.
        /// </summary>
        /// <param name="path">path to source.</param>
        /// <param name="line">line number in source.</param>
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

        /// <inheritdoc/>
        public async override Task ExecuteAsync(TestAdapter adapter, BotCallbackHandler callback, Inspector inspector = null)
        {
            if (Activity == null)
            {
                throw new Exception("You must define one of Text or Activity properties");
            }

            var activity = ObjectPath.Clone(Activity);
            activity.ApplyConversationReference(adapter.Conversation, isIncoming: true);

            if (!string.IsNullOrEmpty(User))
            {
                activity.From = ObjectPath.Clone(activity.From);
                activity.From.Id = User;
                activity.From.Name = User;
            }

            Stopwatch sw = new Stopwatch();
            sw.Start();

            await adapter.ProcessActivityAsync(activity, callback, default).ConfigureAwait(false);

            sw.Stop();
            Trace.TraceInformation($"[Turn Ended => {sw.ElapsedMilliseconds} ms processing UserActivity: {Activity.Text} ]");
        }
    }
}
