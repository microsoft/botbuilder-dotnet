using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Bot.Builder.Dialogs
{
    public static class LGAdapterExtensions
    {
        /// <summary>
        /// Register ILanguageGenerator (and optional IMessageActivityGenerator) will be used by all template binding classes
        /// </summary>
        /// <param name="botAdapter">BotAdapter</param>
        /// <param name="languageGenerator">ILanguageGenerator implementation</param>
        /// <param name="messageGenerator">IMessageActivityGenerator implementation (default is TextMessageActivityGenerator(languageGenerator)</param>
        /// <returns></returns>
        public static BotAdapter UseLanguageGenerator(this BotAdapter botAdapter, ILanguageGenerator languageGenerator, IMessageActivityGenerator messageGenerator = null)
        {
            return botAdapter.Use(new RegisterClassMiddleware<ILanguageGenerator>(languageGenerator))
                    .Use(new RegisterClassMiddleware<IMessageActivityGenerator>(new TextMessageActivityGenerator(languageGenerator)));
        }
    }
}
