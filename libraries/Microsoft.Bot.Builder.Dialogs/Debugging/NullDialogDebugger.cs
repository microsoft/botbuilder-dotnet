using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs.Debugging
{
    /// <summary>
    /// Default Dialog Debugger which simply ignores step calls for the IDialogDebuggerinterface
    /// </summary>
    public class NullDialogDebugger : IDialogDebugger
    {
        public static readonly IDialogDebugger Instance = new NullDialogDebugger();

        private NullDialogDebugger()
        {
        }

        Task IDialogDebugger.StepAsync(DialogContext context, object item, string more, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
