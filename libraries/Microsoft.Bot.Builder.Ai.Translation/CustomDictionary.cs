// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Bot.Builder.Ai.Translation
{
    /// <summary>
    /// A Custom dictionary used to store all the configured user language dictionaries
    /// which in turn will be used in <see cref="CustomDictionaryPostProcessor"/> to overwrite the machine translation output for specific vocab 
    /// with the specific translation in the provided dictionary, 
    /// the <see cref="CustomDictionary"/> contains an internal dictionary of dictionaries inexed by language id,
    /// for ex of how this interal state would look like :
    /// [
    ///     "en", ["court", "courtyard"]
    ///     "it", ["camera", "bedroom"]
    /// ]
    /// as per the last example, the outer dictionary contains all the user configured custom dictionaries indexed by the language id, 
    /// and each internal dictionary contains the <see cref="KeyValuePair{String, String}"/> of this specific language.
    /// </summary>
    public class CustomDictionary
    {
        private readonly Dictionary<string, Dictionary<string, string>> _userCustomDictionaries;

        /// <summary>
        /// Constructs a new <see cref="CustomDictionary"/> object and initializes the internal dictionary variable.
        /// </summary>
        public CustomDictionary()
        {
            _userCustomDictionaries = new Dictionary<string, Dictionary<string, string>>();
        }

        /// <summary>
        /// Adds new custom language dictionary for the set of configured dictionaries.
        /// </summary>
        /// <param name="language"></param>
        /// <param name="dictionary"></param>
        public void AddNewLanguageDictionary(string languageId, Dictionary<string, string> dictionary)
        {
            if (string.IsNullOrWhiteSpace(languageId))
            {
                throw new ArgumentNullException(nameof(languageId));
            }

            if (dictionary == null)
            {
                throw new ArgumentNullException(nameof(dictionary));
            }

            if (_userCustomDictionaries.ContainsKey(languageId))
            {
                throw new ArgumentException(MessagesProvider.ExistingDictionaryErrorMessage);
            }

            _userCustomDictionaries.Add(languageId, dictionary);
        }

        /// <summary>
        /// Get a specific language dictionary using it's key (language id).
        /// </summary>
        /// <param name="languageId"></param>
        /// <returns>A <see cref="Dictionary{String, String}"/> that matches the provided language id</returns>
        public Dictionary<string, string> GetLanguageDictionary(string languageId)
        {
            if (string.IsNullOrWhiteSpace(languageId))
            {
                throw new ArgumentNullException(nameof(languageId));
            }

            if (!_userCustomDictionaries.ContainsKey(languageId))
            {
                throw new ArgumentException(MessagesProvider.NonExistentDictionaryErrorMessage);
            }

            return _userCustomDictionaries[languageId];
        }

        /// <summary>
        /// Check if the <see cref="CustomDictionary"/> object is empty or not.
        /// </summary>
        /// <returns></returns>
        public Boolean IsEmpty()
        {
            return _userCustomDictionaries.Count == 0;
        }
    }
}
