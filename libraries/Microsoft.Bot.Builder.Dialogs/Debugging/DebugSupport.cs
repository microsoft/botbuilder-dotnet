using System.Threading;
using System.Threading.Tasks;
using static Microsoft.Bot.Builder.Dialogs.Debugging.Source;

namespace Microsoft.Bot.Builder.Dialogs.Debugging
{
    /// <summary>
    /// Debugger support for <see cref="ITurnContext"/>, <see cref="DialogContext"/>. 
    /// </summary>
    public static class DebugSupport
    {
        public interface IDebugger
        {
            Task StepAsync(DialogContext context, object item, string more, CancellationToken cancellationToken);
        }

        public static IRegistry SourceRegistry { get; set; } = NullRegistry.Instance;

        public static IDebugger GetDebugger(this ITurnContext context) =>
            context.TurnState.Get<IDebugger>() ?? NullDebugger.Instance;

        public static IDebugger GetDebugger(this DialogContext context) =>
            context.Context.GetDebugger();

        public static async Task DebuggerStepAsync(this DialogContext context, IDialog dialog, string more, CancellationToken cancellationToken)
        {
            await context.GetDebugger().StepAsync(context, dialog, more, cancellationToken).ConfigureAwait(false);
        }

        public static async Task DebuggerStepAsync(this DialogContext context, IRecognizer recognizer, string more, CancellationToken cancellationToken)
        {
            await context.GetDebugger().StepAsync(context, recognizer, more, cancellationToken).ConfigureAwait(false);
        }

        private sealed class NullDebugger : IDebugger
        {
            public static readonly IDebugger Instance = new NullDebugger();

            private NullDebugger()
            {
            }

            Task IDebugger.StepAsync(DialogContext context, object item, string more, CancellationToken cancellationToken)
            {
                return Task.CompletedTask;
            }
        }
    }
}
