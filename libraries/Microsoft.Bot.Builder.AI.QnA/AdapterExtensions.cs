using Microsoft.Bot.Builder.Dialogs.Declarative;

namespace Microsoft.Bot.Builder.AI.QnA
{
    public static class AdapterExtensions
    {
        /// <summary>
        /// Register QnAMaker types 
        /// </summary>
        /// <param name="botAdapter">BotAdapter to add middleware to.</param>
        /// <returns>The bot adapter.</returns>
        public static BotAdapter UseQnAMaker(this BotAdapter botAdapter)
        {
            DeclarativeTypeLoader.AddComponent(new QnAMakerComponentRegistration());
            return botAdapter;
        }
    }
}
