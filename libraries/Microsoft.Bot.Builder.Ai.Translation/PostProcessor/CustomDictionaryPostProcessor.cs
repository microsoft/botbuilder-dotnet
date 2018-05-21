using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Bot.Builder.Ai.Translation.PostProcessor
{
    /// <summary>
    /// Custom dictionary post processor is used to forcibly translate certain vocab from a provided user dictinary.
    /// </summary>
    public class CustomDictionaryPostProcessor : IPostProcessor
    {
        private readonly Dictionary<string, Dictionary<string, string>> _userCustomDictionaries;

        /// <summary>
        /// Constructor using the custom dictionaries map.
        /// </summary>
        /// <param name="userCustomDictionaries">The dictionary/map that stores all the different languages dictionaries keyed by language short name</param>
        public CustomDictionaryPostProcessor(Dictionary<string, Dictionary<string, string>> userCustomDictionaries)
        {
            this._userCustomDictionaries = userCustomDictionaries ?? throw new ArgumentNullException(nameof(userCustomDictionaries));
            if(this._userCustomDictionaries.Count == 0)
            {
                throw new ArgumentException("Custom dictionaries map can't be empty");
            }
        }

        /// <summary>
        /// Process the logic for custom dictionary post processor used to handle user custom vocab translation.
        /// </summary>
        /// <param name="translatedDocument">Translated document</param>
        /// <param name="currentLanguage">Current source language</param>
        /// <returns>A Task represents the asynchronus operation</returns>
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