// Copyright(c) Microsoft Corporation.All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using AdaptiveExpressions;
using Microsoft.Bot.Builder.Adapters;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Testing.TestActions
{
    /// <summary>
    /// Run assertions against memory.
    /// </summary>
    [DebuggerDisplay("MemoryAssertions")]
    public class MemoryAssertions : TestAction
    {
        /// <summary>
        /// Kind for the serialization.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.Test.MemoryAssertions";

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryAssertions"/> class.
        /// </summary>
        /// <param name="path">Path to source.</param>
        /// <param name="line">Line number in source.</param>
        [JsonConstructor]
        public MemoryAssertions([CallerFilePath] string path = "", [CallerLineNumber] int line = 0)
        {
            RegisterSourcePath(path, line);
        }

        /// <summary>
        /// Gets or sets the description of this assertion.
        /// </summary>
        /// <value>Description of what this assertion is.</value>
        [JsonProperty("description")]
        public string Description { get; set; }

        /// <summary>
        /// Gets the assertions.
        /// </summary>
        /// <value>The assertion expressions.</value>
        [JsonProperty("assertions")]
        public List<string> Assertions { get; } = new List<string>();

        /// <inheritdoc/>
        public async override Task ExecuteAsync(TestAdapter adapter, BotCallbackHandler callback, Inspector inspector = null)
        {
            if (inspector != null)
            {
                await inspector((dc) =>
                    {
                        foreach (var assertion in Assertions)
                        {
                            var (val, error) = Expression.Parse(assertion).TryEvaluate<bool>(dc.State);
                            if (error != null || !val)
                            {
                                throw new InvalidOperationException($"{Description} {assertion} MemoryAssertion failed");
                            }
                        }
                    }).ConfigureAwait(false);
                Trace.TraceInformation($"[Turn Ended => {Description} MemoryAssertions passed]");
            }
            else
            {
                Trace.TraceInformation($"[Turn Ended => No inspector for {Description} MemoryAssertions]");
            }
        }
    }
}
