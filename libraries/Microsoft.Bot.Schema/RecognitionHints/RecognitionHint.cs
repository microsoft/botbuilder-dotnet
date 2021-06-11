// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using Newtonsoft.Json;

namespace Microsoft.Bot.Schema
{
    /// <summary>
    /// Hint for recognizing input.
    /// </summary>
    public class RecognitionHint
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RecognitionHint"/> class.
        /// </summary>
        /// <param name="type">Type of recognition hint.</param>
        /// <param name="name">Name for this hint.</param>
        public RecognitionHint(string type, string name)
        {
            Type = type;
            Name = name;
        }

        /// <summary>
        /// Gets the type of this hint.
        /// </summary>
        /// <value>Type of recognition hint.</value>
        [JsonProperty("type")]
        public string Type { get; }

        /// <summary>
        /// Gets the name for this hint.
        /// </summary>
        /// <remarks>The interpetation of the name is determined by the type.</remarks>
        /// <value>Name of hint.</value>
        [JsonProperty("name")]
        public string Name { get; }

        /// <summary>
        /// Gets or sets the importance of this hint.
        /// </summary>
        /// <value>Importance usually from <see cref="RecognitionHintImportance"/>.</value>
        [JsonProperty("importance")]
        public string Importance { get; set; }

        /// <summary>
        /// Returns the string corresponding to a <see cref="RecognitionHintImportance"/>.
        /// </summary>
        /// <param name="importance">Enum selection.</param>
        /// <returns>String version of enumeration.</returns>
        public static string ImportanceString(RecognitionHintImportance importance)
            => importance.ToString().ToLowerInvariant();
        
        /// <summary>
        /// Clone the recognition hint.
        /// </summary>
        /// <returns>A shallow copy of the hint.</returns>
        public virtual RecognitionHint Clone()
            => new RecognitionHint(Type, Name) { Importance = Importance };

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{ToStringPrefix()}{Type}:{Name}";
        }

        /// <summary>
        /// Importance marker in ToString.
        /// </summary>
        /// <returns>Marker for importance.</returns>
        protected string ToStringPrefix()
            => Importance == RecognitionHint.ImportanceString(RecognitionHintImportance.Expected) ? "+" : string.Empty;
    }
}
