using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveExpressions.Properties;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Actions
{
    /// <summary>
    /// Send an handoff activity.
    /// </summary>
    public class SendHandoffActivity : Dialog
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.SendHandoffActivity";

        /// <summary>
        /// Initializes a new instance of the <see cref="SendHandoffActivity"/> class.
        /// </summary>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        public SendHandoffActivity([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base()
        {
            // enable instances of this command as debug break point
            this.RegisterSourceLocation(callerPath, callerLine);
        }

        /// <summary>
        /// Gets or sets the context object to be included when sending the handoff activity.
        /// </summary>
        /// <value>
        /// <see cref="object"/>.
        /// </value>
        [JsonProperty("context")]
        public ObjectExpression<object> HandoffContext { get; set; }

        /// <summary>
        /// Gets or sets the Transcript object to be included when sending the handoff activity.
        /// </summary>
        /// <value>
        /// <see cref="Transcript"/>.
        /// </value>
        [JsonProperty("transcript")]
        public ObjectExpression<Transcript> Transcript { get; set; }

        /// <summary>
        /// Called when the dialog is started and pushed onto the dialog stack.
        /// </summary>
        /// <param name="dc">The <see cref="DialogContext"/> for the current turn of conversation.</param>
        /// <param name="options">Optional, initial information to pass to the dialog.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var handoffContext = HandoffContext?.GetValue(dc.State);
            var transcript = Transcript?.GetValue(dc.State);
            var eventActivity = EventFactory.CreateHandoffInitiation(dc.Context, handoffContext, transcript);
            await dc.Context.SendActivityAsync(eventActivity, cancellationToken).ConfigureAwait(false);
            return await dc.EndDialogAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
        }
    }
}
