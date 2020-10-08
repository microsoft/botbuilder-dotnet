// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Testing.TestActions
{
    /// <summary>
    /// Script action to delay test script for specified timespan.
    /// </summary>
    [DebuggerDisplay("UserDelay:{Timespan}")]
    public class UserDelay : TestAction
    {
        /// <summary>
        /// serialization of kind.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.Test.UserDelay";

        /// <summary>
        /// Initializes a new instance of the <see cref="UserDelay"/> class.
        /// </summary>
        /// <param name="path">path for source.</param>
        /// <param name="line">line number in source.</param>
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

        /// <inheritdoc/>
        public async override Task ExecuteAsync(TestAdapter adapter, BotCallbackHandler callback, Inspector inspector = null)
        {
            await Task.Delay((int)Timespan).ConfigureAwait(false);
            Trace.TraceInformation($"[Turn Ended => {Timespan} ms processing UserDelay[{Timespan}]");
        }
    }
}
