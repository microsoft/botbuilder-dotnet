using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Bot.Builder.Dialogs.Form;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive
{
    public static class FormAdapterExtensions
    {
        /// <summary>
        /// Register ResourceExplorer and optionally register more types.
        /// </summary>
        /// <param name="botAdapter">BotAdapter to add middleware to.</param>
        /// <returns>The bot adapter.</returns>
        public static BotAdapter UseFormDialogs(this BotAdapter botAdapter)
        {
            DeclarativeTypeLoader.AddComponent(new FormComponentRegistration());
            return botAdapter;
        }
    }
}
