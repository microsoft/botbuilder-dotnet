using System;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// MainDialog handles the basic structure of running the root of a dialog stack.
    /// </summary>
    public class MainDialog : ComponentDialog
    {
        private DialogSet mainDialogSet;

        public MainDialog(IStatePropertyAccessor<DialogState> dialogState, string dialogId = nameof(MainDialog))
            : base(dialogId)
        {
            this.DialogStateProperty = dialogState ?? throw new ArgumentNullException(nameof(dialogState));

            // create dialog set for the outer dialog
            this.mainDialogSet = new DialogSet(this.DialogStateProperty)
                .Add(this);
        }

        /// <summary>
        /// gets or sets the DialogState property Accessor.
        /// </summary>
        protected IStatePropertyAccessor<DialogState> DialogStateProperty { get; set; }

        /// <summary>
        /// Run the current dialog stack.
        /// </summary>
        /// <param name="turnContext">turn context</param>
        /// <returns>result of running the current main dialog</returns>
        public async Task<DialogTurnResult> RunAsync(ITurnContext turnContext)
        {
            if (turnContext == null)
            {
                throw new ArgumentNullException(nameof(turnContext));
            }

            // Create a dialog context and try to continue running the current dialog
            var dialogContext = await this.mainDialogSet.CreateContextAsync(turnContext).ConfigureAwait(false);

            // continue any running dialog
            var result = await dialogContext.ContinueDialogAsync().ConfigureAwait(false);

            // if there isn't one running, then start main dialog as the root
            if (result.Status == DialogTurnStatus.Empty)
            {
                result = await dialogContext.BeginDialogAsync(this.Id).ConfigureAwait(false);
            }

            return result;
        }
    }
}
