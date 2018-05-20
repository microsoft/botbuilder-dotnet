using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Bot.Builder.Ai.Translation.PostProcessor
{
    public class CustomDictionaryPostProcessor : IPostProcessor
    {
        private readonly Dictionary<string, Dictionary<string, string>> _userCustomDictionaries;

        public CustomDictionaryPostProcessor(Dictionary<string, Dictionary<string, string>> userCustomDictionaries)
        {
            if (userCustomDictionaries == null)
            {
                throw new ArgumentNullException(nameof(userCustomDictionaries));
            }
            this._userCustomDictionaries = userCustomDictionaries;
        }

        public PostProcessedDocument Process(TranslatedDocument translatedDocument, string currentLanguage)
        {
            if (_userCustomDictionaries[currentLanguage].Count > 0)
            {
                for (int i = 0; i < translatedDocument.SourceTokens.Length; i++)
                {
                    if (_userCustomDictionaries[currentLanguage].ContainsKey(translatedDocument.SourceTokens[i]))
                    {
                        translatedDocument.TranslatedTokens[translatedDocument.IndexedAlignment[i]] = _userCustomDictionaries[currentLanguage][translatedDocument.SourceTokens[i]];
                    }
                }
                return new PostProcessedDocument(translatedDocument, string.Join(" ", translatedDocument.TranslatedTokens));
            }
            else
            {
                return new PostProcessedDocument(translatedDocument, string.Empty);
            }
        }
    }
}