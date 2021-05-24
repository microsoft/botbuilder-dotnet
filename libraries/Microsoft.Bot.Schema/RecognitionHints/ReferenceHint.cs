// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using Newtonsoft.Json;

namespace Microsoft.Bot.Schema
{
    /// <summary>
    /// Hint from an LU source file.
    /// </summary>
    public class ReferenceHint : RecognitionHint
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReferenceHint"/> class.
        /// </summary>
        /// <param name="name">Name in resource.</param>
        /// <param name="resource">Resource.</param>
        /// <remarks>
        /// Resource is usually something like an LU file name.
        /// </remarks>
        public ReferenceHint(string name, string resource)
            : base("Reference", name)
        {
            Resource = resource;
        }

        /// <summary>
        /// Gets the source LU file where <see cref="RecognitionHint.Name"/> is found.
        /// </summary>
        /// <value>Name of the LU file like MyApp.lu.</value>
        [JsonProperty("source")]
        public string Resource { get;  }

        /// <inheritdoc/>
        public override RecognitionHint Clone()
            => new ReferenceHint(Name, Resource) { Importance = Importance };

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{ToStringPrefix()}{Resource}/{Name}";
        }
    }
}
