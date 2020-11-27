// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveExpressions;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Testing.TestActions
{
    /// <summary>
    /// Basic assertion TestAction, which validates assertions against a reply activity.
    /// </summary>
    [DebuggerDisplay("AssertReplyActivity:{GetConditionDescription()}")]
    public class AssertReplyActivity : TestAction
    {
        /// <summary>
        /// Kind for json serialization.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.Test.AssertReplyActivity";

        /// <summary>
        /// Initializes a new instance of the <see cref="AssertReplyActivity"/> class.
        /// </summary>
        /// <param name="path">optional path.</param>
        /// <param name="line">optional line.</param>
        [JsonConstructor]
        public AssertReplyActivity([CallerFilePath] string path = "", [CallerLineNumber] int line = 0)
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
        /// Gets or sets the milliseconds to wait for a reply.
        /// </summary>
        /// <value>the milliseceods to wait.</value>
        [DefaultValue(3000)]
        [JsonProperty("timeout")]
        public uint Timeout { get; set; } = 3000;

        /// <summary>
        /// Gets the assertions.
        /// </summary>
        /// <value>The expressions for assertions.</value>
        [JsonProperty("assertions")]
        public List<string> Assertions { get; } = new List<string>();

        /// <summary>
        /// Gets the text to assert for an activity.
        /// </summary>
        /// <returns>String.</returns>
        public virtual string GetConditionDescription()
        {
            return Description ?? string.Join("\n", Assertions);
        }

        /// <summary>
        /// Validates the reply of an activity.
        /// </summary>
        /// <param name="activity">The activity to verify.</param>
        public virtual void ValidateReply(Activity activity)
        {
            foreach (var assertion in Assertions)
            {
                var (result, error) = Expression.Parse(assertion).TryEvaluate<bool>(activity);
                if (result != true)
                {
                    throw new InvalidOperationException($"{Description} {assertion}\n{JsonConvert.SerializeObject(activity, Formatting.Indented)}");
                }
            }
        }

        /// <inheritdoc/>
        public override async Task ExecuteAsync(TestAdapter adapter, BotCallbackHandler callback, Inspector inspector = null)
        {
            var timeout = (int)Timeout;

            if (Debugger.IsAttached)
            {
                timeout = int.MaxValue;
            }

            using (var cts = new CancellationTokenSource())
            {
                cts.CancelAfter((int)timeout);
                var replyActivity = await adapter.GetNextReplyAsync(cts.Token).ConfigureAwait(false);

                if (replyActivity != null)
                {
                    ValidateReply((Activity)replyActivity);
                    return;
                }
            }
        }
    }
}
