using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.TestBot.Json
{
    /// <summary>
    /// Custom command which takes takes 2 data bound arguments (arg1 and arg2) and multiplies them returning that as a databound result.
    /// </summary>
    public class MultiplyAction : DialogAction
    {
        [JsonConstructor]
        public MultiplyAction([CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
            : base()
        {
            // enable instances of this command as debug break point
            this.RegisterSourceLocation(sourceFilePath, sourceLineNumber);
        }

        /// <summary>
        /// Gets or sets memory path to bind to arg1 (ex: conversation.width).
        /// </summary>
        /// <value>
        /// memory path to bind to arg1 (ex: conversation.width).
        /// </value>
        [JsonProperty("arg1")]
        public string Arg1
        {
            get { return this.InputBindings["arg1"]; } set { this.InputBindings["arg1"] = value; }
        }

        /// <summary>
        /// Gets or sets memory path to bind to arg2 (ex: conversation.height).
        /// </summary>
        /// <value>
        /// memory path to bind to arg2 (ex: conversation.height).
        /// </value>
        [JsonProperty("arg2")]
        public string Arg2
        {
            get { return this.InputBindings["arg2"]; } set { this.InputBindings["arg2"] = value; }
        }

        /// <summary>
        /// Gets or sets caller's memory path to store the result of this step in (ex: conversation.area).
        /// </summary>
        /// <value>
        /// caller's memory path to store the result of this step in (ex: conversation.area).
        /// </value>
        [JsonProperty("result")]
        public string Result
        {
            get { return this.OutputBinding; } set { this.OutputBinding = value; }
        }

        protected override Task<DialogTurnResult> OnRunCommandAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            // get the values that were data bound from parents memory context using the InputBindings
            var arg1 = dc.State.GetValue<float>($"dialog.result.arg1");
            var arg2 = dc.State.GetValue<float>($"dialog.result.arg2");
            var result = arg1 * arg2;

            // result will be databound to parents memory using the OutputBinding value
            return dc.EndDialogAsync(result: result, cancellationToken: cancellationToken);
        }
    }
}
