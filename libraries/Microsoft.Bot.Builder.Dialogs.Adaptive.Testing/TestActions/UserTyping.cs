﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Testing.TestActions
{
    /// <summary>
    /// Action to script sending typing activity to bot.
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("UserTyping")]
    public class UserTyping : TestAction
    {
        /// <summary>
        /// Kind to serialize.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.Test.UserTyping";

        /// <summary>
        /// Initializes a new instance of the <see cref="UserTyping"/> class.
        /// </summary>
        /// <param name="path">path for source.</param>
        /// <param name="line">line number in source.</param>
        [JsonConstructor]
        public UserTyping([CallerFilePath] string path = "", [CallerLineNumber] int line = 0)
        {
            RegisterSourcePath(path, line);
        }

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
            var typing = adapter.MakeActivity();
            typing.Type = ActivityTypes.Typing;

            if (!string.IsNullOrEmpty(User))
            {
                typing.From = ObjectPath.Clone(typing.From);
                typing.From.Id = User;
                typing.From.Name = User;
            }

            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            await adapter.ProcessActivityAsync((Activity)typing, callback, default).ConfigureAwait(false);

            sw.Stop();

            System.Diagnostics.Trace.TraceInformation($"[Turn Ended => {sw.ElapsedMilliseconds} ms processing UserConversationUpdate[]");
        }
    }
}
