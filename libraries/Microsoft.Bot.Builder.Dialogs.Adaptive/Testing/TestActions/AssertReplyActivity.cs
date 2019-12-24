using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Dialogs.Debugging;
using Microsoft.Bot.Expressions;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Testing.Actions
{
    /// <summary>
    /// Basic assertion TestAction, which validates assertions against a reply activity.
    /// </summary>
    [DebuggerDisplay("AssertReplyActivity:{GetConditionDescription()}")]
    public class AssertReplyActivity : TestAction
    {
        [JsonProperty("$kind")]
        public const string DeclarativeType = "Microsoft.Test.AssertReplyActivity";

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
                var engine = new ExpressionEngine();
                foreach (var assertion in this.Assertions)
                {
                    var (result, error) = engine.Parse(assertion).TryEvaluate(activity);
                    if ((bool)result != true)
                    {
                        throw new Exception($"{this.Description} {assertion} {activity}");
                    }
                }
            }
        }

        public async override Task ExecuteAsync(TestAdapter adapter, BotCallbackHandler callback)
        {
            var timeout = Timeout;

            //if (System.Diagnostics.Debugger.IsAttached)
            //{
            //    timeout = uint.MaxValue;
            //}

            var start = DateTime.UtcNow;
            while (true)
            {
                var current = DateTime.UtcNow;

                if ((current - start).TotalMilliseconds > timeout)
                {
                    throw new TimeoutException($"{timeout}ms Timed out waiting for: {GetConditionDescription()}");
                }

                IActivity replyActivity = adapter.GetNextReply();
                if (replyActivity != null)
                {
                    ValidateReply((Activity)replyActivity);
                    return;
                }

                await Task.Delay(100).ConfigureAwait(false);
            }
        }
    }
}
