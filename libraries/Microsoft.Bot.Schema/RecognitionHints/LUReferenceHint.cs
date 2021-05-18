// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using Newtonsoft.Json;

namespace Microsoft.Bot.Schema
{
    /// <summary>
    /// Hint from an LU source file.
    /// </summary>
    public class LUReferenceHint : RecognitionHint
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LUReferenceHint"/> class.
        /// </summary>
        /// <param name="name">Name of LU file definition.</param>
        /// <param name="source">Source LU file.</param>
        public LUReferenceHint(string name, string source)
            : base("LUReference", name)
        {
            Source = source;
        }

        /// <summary>
        /// Gets the source LU file where <see cref="RecognitionHint.Name"/> is found.
        /// </summary>
        /// <value>Name of the LU file like MyApp.lu.</value>
        [JsonProperty("source")]
        public string Source { get;  }

        /// <inheritdoc/>
        public override RecognitionHint Clone()
            => new LUReferenceHint(Name, Source) { Importance = Importance };

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{ToStringPrefix()}{Source}/{Name}";
        }
    }
}
