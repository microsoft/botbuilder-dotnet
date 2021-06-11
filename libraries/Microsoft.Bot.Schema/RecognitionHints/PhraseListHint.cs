// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Microsoft.Bot.Schema
{
    /// <summary>
    /// Hint for recognizing input.
    /// </summary>
    public class PhraseListHint : RecognitionHint
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PhraseListHint"/> class.
        /// </summary>
        /// <param name="name">Name for the phrase list.</param>
        /// <param name="phrases">Source LU file.</param>
        public PhraseListHint(string name, IEnumerable<string> phrases)
            : base("phraselist", name)
        {
            Phrases = phrases.ToList();
        }

        /// <summary>
        /// Gets a list of phrases that could be included in input.
        /// </summary>
        /// <value>Phrase list.</value>
        [JsonProperty("phrases")]
        public IReadOnlyList<string> Phrases { get;  }

        /// <inheritdoc/>
        public override RecognitionHint Clone()
            => new PhraseListHint(Name, Phrases) { Importance = Importance };

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{ToStringPrefix()}{Name}[{Phrases.Count}]";
        }
    }
}
