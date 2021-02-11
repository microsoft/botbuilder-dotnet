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
    /// Action to script sending custom event to the bot.
    /// </summary>
    [DebuggerDisplay("CustomEvent:{Name}")]
    public class CustomEvent : TestAction
    {
        /// <summary>
        /// Seralization of kind.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.Test.CustomEvent";

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomEvent"/> class.
        /// </summary>
        /// <param name="path">path for source.</param>
        /// <param name="line">line number in source.</param>
        [JsonConstructor]
        public CustomEvent([CallerFilePath] string path = "", [CallerLineNumber] int line = 0)
        {
            RegisterSourcePath(path, line);
        }

        /// <summary>
        /// Gets or sets the event name.
        /// </summary>
        /// <value>
        /// The event name.
        /// </value>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the event value.
        /// </summary>
        /// <value>
        /// Event value.
        /// </value>
        [JsonProperty("value")]
        public object Value { get; set; }

        /// <inheritdoc/>
        public async override Task ExecuteAsync(TestAdapter adapter, BotCallbackHandler callback, Inspector inspector = null)
        {
            if (Name == null)
            {
                throw new InvalidOperationException("You must define the event name.");
            }

            var eventActivity = adapter.MakeActivity();
            eventActivity.Type = ActivityTypes.Event;
            eventActivity.Name = Name;
            eventActivity.Value = Value;

            Stopwatch sw = new Stopwatch();
            sw.Start();
            await adapter.ProcessActivityAsync(eventActivity, callback, default).ConfigureAwait(false);
            sw.Stop();
            Trace.TraceInformation($"[Turn Ended => {sw.ElapsedMilliseconds} ms processing CustomEvent: {Name} ]");
        }
    }
}
