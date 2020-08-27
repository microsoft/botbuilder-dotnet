using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveExpressions;
using AdaptiveExpressions.Properties;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Testing.Actions
{
    /// <summary>
    /// Dialog action which allows you to add assertions into your dialog flow.
    /// </summary>
    public class AssertCondition : Dialog
    {
        /// <summary>
        /// Kind to use for serialization.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.Test.AssertCondition";

        /// <summary>
        /// Initializes a new instance of the <see cref="AssertCondition"/> class.
        /// </summary>
        /// <param name="path">optional path.</param>
        /// <param name="line">optional line.</param>
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

        /// <summary>
        /// Begins the dialog.
        /// </summary>
        /// <param name="dc">The DialogContext.</param>
        /// <param name="options">Options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>DialogTurnResult.</returns>
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
