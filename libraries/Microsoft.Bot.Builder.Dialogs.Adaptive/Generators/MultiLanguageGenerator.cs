// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Concurrent;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Generators
{
    /// <summary>
    /// ILanguageGenerator which uses implements a map of locale->ILanguageGenerator for the locale and has a policy which controls fallback (try en-us -> en -> default).
    /// </summary>
    public class MultiLanguageGenerator : MultiLanguageGeneratorBase
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.MultiLanguageGenerator";

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiLanguageGenerator"/> class.
        /// </summary>
        public MultiLanguageGenerator()
        {
        }

        /// <summary>
        /// Gets the language generators for multiple languages.
        /// </summary>
        /// <value>
        /// The language generators for multiple languages.
        /// </value>
        [JsonProperty("languageGenerators")]
        public ConcurrentDictionary<string, Lazy<LanguageGenerator>> LanguageGenerators { get; } = new ConcurrentDictionary<string, Lazy<LanguageGenerator>>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Implementation of lookup by locale.  This uses internal dictionary to lookup.
        /// </summary>
        /// <param name="dialogContext">Context for the current turn of conversation with the user.</param>\
        /// <param name="locale">locale.</param>
        /// <param name="languageGenerator">generator to return.</param>
        /// <returns>true if found.</returns>
        public override bool TryGetGenerator(DialogContext dialogContext, string locale, out Lazy<LanguageGenerator> languageGenerator)
        {
            return this.LanguageGenerators.TryGetValue(locale, out languageGenerator);
        }
    }
}
