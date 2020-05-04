// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Testing.TestActions
{
    [DebuggerDisplay("UserTyping")]
    public class UserTyping : TestAction
    {
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.Test.UserTyping";

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

        public async override Task ExecuteAsync(TestAdapter adapter, BotCallbackHandler callback)
        {
            var typing = adapter.MakeActivity();
            typing.Type = ActivityTypes.Typing;

            if (!string.IsNullOrEmpty(this.User))
            {
                typing.From = ObjectPath.Clone(typing.From);
                typing.From.Id = this.User;
                typing.From.Name = this.User;
            }

            Stopwatch sw = new Stopwatch();
            sw.Start();

            await adapter.ProcessActivityAsync((Activity)typing, callback, default(CancellationToken)).ConfigureAwait(false);

            sw.Stop();

            Trace.TraceInformation($"[Turn Ended => {sw.ElapsedMilliseconds} ms processing UserConversationUpdate[]");
        }
    }
}
