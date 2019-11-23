// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Testing
{
    [DebuggerDisplay("UserDelay:{Timespan}")]
    public class UserDelay : TestAction
    {
        [JsonProperty("$kind")]
        public const string DeclarativeType = "Microsoft.Test.UserDelay";

        [JsonConstructor]
        public UserDelay([CallerFilePath] string path = "", [CallerLineNumber] int line = 0)
        {
            RegisterSourcePath(path, line);
        }

        /// <summary>
        /// Gets or sets the timespan to delay.
        /// </summary>
        /// <value>
        /// The timespan to delay.
        /// </value>
        [JsonProperty("timespan")]
        public uint Timespan { get; set; }

        public async override Task ExecuteAsync(TestAdapter adapter, BotCallbackHandler callback)
        {
            await Task.Delay((int)Timespan).ConfigureAwait(false);
        }
    }
}
