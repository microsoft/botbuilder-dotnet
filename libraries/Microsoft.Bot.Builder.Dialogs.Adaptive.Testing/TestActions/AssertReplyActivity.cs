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
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.Test.AssertReplyActivity";

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
        /// Gets or sets the assertions.
        /// </summary>
        /// <value>The expressions for assertions.</value>
        [JsonProperty("assertions")]
        public List<string> Assertions { get; set; }

        public virtual string GetConditionDescription()
        {
            return Description ?? string.Join("\n", Assertions);
        }

        public virtual void ValidateReply(Activity activity)
        {
            if (this.Assertions != null)
            {
                foreach (var assertion in this.Assertions)
                {
                    var (result, error) = Expression.Parse(assertion).TryEvaluate<bool>(activity);
                    if (result != true)
                    {
                        throw new Exception($"{this.Description} {assertion} {activity}");
                    }
                }
            }
        }

        public async override Task ExecuteAsync(TestAdapter adapter, BotCallbackHandler callback)
        {
            var timeout = (int)this.Timeout;

            if (System.Diagnostics.Debugger.IsAttached)
            {
                timeout = int.MaxValue;
            }

            CancellationTokenSource cts = new CancellationTokenSource();
            cts.CancelAfter((int)timeout);
            IActivity replyActivity = await adapter.GetNextReplyAsync(cts.Token).ConfigureAwait(false);

            if (replyActivity != null)
            {
                ValidateReply((Activity)replyActivity);
                return;
            }
        }
    }
}
