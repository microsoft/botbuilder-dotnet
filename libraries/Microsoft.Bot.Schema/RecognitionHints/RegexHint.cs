// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using Newtonsoft.Json;

namespace Microsoft.Bot.Schema
{
    /// <summary>
    /// Hint from a regex.
    /// </summary>
    public class RegexHint : RecognitionHint
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RegexHint"/> class.
        /// </summary>
        /// <param name="name">Name of the regex.</param>
        /// <param name="pattern">Regular expression.</param>
        public RegexHint(string name, string pattern)
            : base("regex", name)
        {
            Pattern = pattern;
        }

        /// <summary>
        /// Gets the source LU file where <see cref="RecognitionHint.Name"/> is found.
        /// </summary>
        /// <value>Name of the LU file like MyApp.lu.</value>
        [JsonProperty("pattern")]
        public string Pattern { get;  }

        /// <inheritdoc/>
        public override RecognitionHint Clone()
            => new RegexHint(Name, Pattern) { Importance = Importance };

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{ToStringPrefix()}{Name}/{Pattern}/";
        }
    }
}
