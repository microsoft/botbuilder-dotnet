namespace Microsoft.Bot.Builder.Dialogs
{
    public class DialogContextPath
    {
        /// <summary>
        /// Memory Path to dialogContext's active dialog.
        /// </summary>
        public const string ActiveDialog = "dialogcontext.activeDialog";

        /// <summary>
        /// Memory Path to dialogContext's parent dialog.
        /// </summary>
        public const string Parent = "dialogcontext.parent";

        /// <summary>
        /// Memory Path to dialogContext's stack.
        /// </summary>
        public const string Stack = "dialogContext.stack";
    }
}
