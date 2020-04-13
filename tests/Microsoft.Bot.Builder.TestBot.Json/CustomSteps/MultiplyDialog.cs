using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveExpressions;
using Microsoft.Bot.Builder.Dialogs;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.TestBot.Json
{
    /// <summary>
    /// Custom command which takes takes 2 data bound arguments (arg1 and arg2) and multiplies them returning that as a databound result.
    /// </summary>
    public class MultiplyDialog : Dialog
    {
        [JsonConstructor]
        public MultiplyDialog([CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
            : base()
        {
            // enable instances of this command as debug break point
            this.RegisterSourceLocation(sourceFilePath, sourceLineNumber);
        }

        /// <summary>
        /// Gets or sets memory path to bind to arg1 (ex: conversation.width).
        /// </summary>
        /// <value>
        /// Memory path to bind to arg1 (ex: conversation.width).
        /// </value>
        [JsonProperty("arg1")]
        public string Arg1 { get; set; }

        /// <summary>
        /// Gets or sets memory path to bind to arg2 (ex: conversation.height).
        /// </summary>
        /// <value>
        /// Memory path to bind to arg2 (ex: conversation.height).
        /// </value>
        [JsonProperty("arg2")]
        public string Arg2 { get; set; }

        /// <summary>
        /// Gets or sets caller's memory path to store the result of this step in (ex: conversation.area).
        /// </summary>
        /// <value>
        /// Caller's memory path to store the result of this step in (ex: conversation.area).
        /// </value>
        [JsonProperty("resultProperty")]
        public string ResultProperty { get; set; }

        public override Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var (arg1, err1) = Expression.Parse(Arg1).TryEvaluate(dc.State);
            var (arg2, err2) = Expression.Parse(Arg2).TryEvaluate(dc.State);

            var result = Convert.ToInt32(arg1) * Convert.ToInt32(arg2);
            if (this.ResultProperty != null)
            {
                dc.State.SetValue(this.ResultProperty, result);
            }

            return dc.EndDialogAsync(result: result, cancellationToken: cancellationToken);
        }
    }
}
