using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using Microsoft.Bot.Builder.Dialogs;

namespace Microsoft.Bot.Builder.LanguageGeneration
{

    /// <summary>
    /// Use dictionary of locale -> ILanguageGenerator instances
    /// </summary>
    public class MultiLanguageGenerator : MultiLanguageGeneratorBase
    {

        public MultiLanguageGenerator()
        {
        }


        public override bool TryGetGenerator(ITurnContext context, string locale, out ILanguageGenerator languageGenerator)
        {
            return this.LanguageGenerators.TryGetValue(locale, out languageGenerator);
        }

        public ConcurrentDictionary<string, ILanguageGenerator> LanguageGenerators = new ConcurrentDictionary<string, ILanguageGenerator>(StringComparer.OrdinalIgnoreCase);
    }
}
