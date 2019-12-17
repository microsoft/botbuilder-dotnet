// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.LanguageGeneration;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers
{
    /// <summary>
    /// Defines map of languages -> recognizer.
    /// </summary>
    public class MultiLanguageRecognizer : InputRecognizer
    {
        [JsonProperty("$kind")]
        public const string DeclarativeType = "Microsoft.MultiLanguageRecognizer";

        [JsonConstructor]
        public MultiLanguageRecognizer()
        {
        }

        /// <summary>
        /// Gets or sets policy for languages fallback. 
        /// </summary>
        /// <value>
        /// Policy for languages fallback. 
        /// </value>
        [JsonProperty("languagePolicy")]
        public LanguagePolicy LanguagePolicy { get; set; } = new LanguagePolicy();

        /// <summary>
        /// Gets or sets map of languages -> IRecognizer.
        /// </summary>
        /// <value>
        /// Map of languages -> IRecognizer.
        /// </value>
        [JsonProperty("recognizers")]
        public IDictionary<string, InputRecognizer> Recognizers { get; set; } = new Dictionary<string, InputRecognizer>();

        public override async Task<RecognizerResult> RecognizeAsync(DialogContext dialogContext, string text, string locale, CancellationToken cancellationToken)
        {
            if (!LanguagePolicy.TryGetValue(locale ?? string.Empty, out string[] policy))
            {
                policy = new string[] { string.Empty };
            }

            foreach (var option in policy)
            {
                if (this.Recognizers.TryGetValue(option, out var recognizer))
                {
                    return await recognizer.RecognizeAsync(dialogContext, text, locale, cancellationToken).ConfigureAwait(false);
                }
            }

            // nothing recognized
            return new RecognizerResult() { };
        }
    }
}
