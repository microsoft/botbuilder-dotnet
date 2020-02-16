// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.LanguageGeneration;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers
{
    /// <summary>
    /// Defines map of languages -> recognizer.
    /// </summary>
    public class MultiLanguageRecognizer : Recognizer
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
        public IDictionary<string, Recognizer> Recognizers { get; set; } = new Dictionary<string, Recognizer>();

        public override async Task<RecognizerResult> RecognizeAsync(DialogContext dialogContext, Activity activity, CancellationToken cancellationToken)
        {
            if (!LanguagePolicy.TryGetValue(activity.Locale ?? string.Empty, out string[] policy))
            {
                policy = new string[] { string.Empty };
            }

            foreach (var option in policy)
            {
                if (this.Recognizers.TryGetValue(option, out var recognizer))
                {
                    return await recognizer.RecognizeAsync(dialogContext, activity, cancellationToken).ConfigureAwait(false);
                }
            }

            // nothing recognized
            return new RecognizerResult() { };
        }
    }
}
