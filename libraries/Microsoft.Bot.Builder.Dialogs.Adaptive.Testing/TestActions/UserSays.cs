// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Testing.TestActions
{
    /// <summary>
    /// Action to script sending text to the bot.
    /// </summary>
    [DebuggerDisplay("UserSays:{Text}")]
    public class UserSays : TestAction
    {
        /// <summary>
        /// Seralization of kind.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.Test.UserSays";

        /// <summary>
        /// Initializes a new instance of the <see cref="UserSays"/> class.
        /// </summary>
        /// <param name="path">path for source.</param>
        /// <param name="line">line number in source.</param>
        [JsonConstructor]
        public UserSays([CallerFilePath] string path = "", [CallerLineNumber] int line = 0)
        {
            RegisterSourcePath(path, line);
        }

        /// <summary>
        /// Gets or sets the text.
        /// </summary>
        /// <value>
        /// the text to send to the bot.
        /// </value>
        [JsonProperty("text")]
        public string Text { get; set; }

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
            if (Text == null)
            {
                throw new Exception("You must define the Text property");
            }

            var activity = adapter.MakeActivity(Text);
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
            Trace.TraceInformation($"[Turn Ended => {sw.ElapsedMilliseconds} ms processing UserSays: {Text} ]");
        }
    }
}
