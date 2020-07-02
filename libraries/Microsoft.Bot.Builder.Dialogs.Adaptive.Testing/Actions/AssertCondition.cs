using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveExpressions;
using AdaptiveExpressions.Properties;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Testing.Actions
{
    public class AssertCondition : Dialog
    {
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.Test.AssertCondition";
        
        [JsonConstructor]
        public AssertCondition([CallerFilePath] string path = "", [CallerLineNumber] int line = 0)
        {
            RegisterSourceLocation(path, line);
        }

        /// <summary>
        /// Gets or sets condition which must be true.
        /// </summary>
        /// <value>
        /// Condition which must be true.
        /// </value>
        [JsonProperty("condition")]
        public Expression Condition { get; set; }

        /// <summary>
        /// Gets or sets description of assertion.
        /// </summary>
        /// <value>
        /// Description of assertion.
        /// </value>
        [JsonProperty("description")]
        public StringExpression Description { get; set; }

        public async override Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default)
        {
            var (result, error) = Condition.TryEvaluate(dc.State);
            if ((bool)result == false)
            {
                var desc = Description?.GetValue(dc.State) ?? Condition.ToString();
                throw new Exception(desc);
            }

            return await dc.EndDialogAsync().ConfigureAwait(false);
        }
    }
}
