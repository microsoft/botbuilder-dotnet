// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using Newtonsoft.Json;

namespace Microsoft.Bot.Schema
{
    /// <summary>
    /// Description of an intent.
    /// </summary>
    public partial class IntentDescription
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IntentDescription"/> class.
        /// </summary>
        /// <param name="name">Intent name.</param>
        /// <param name="source">Source of intent definition.</param>
        public IntentDescription(string name, string source = null)
        {
            Name = name;
            Source = source;
        }

        /// <summary>
        /// Gets name of the intent.
        /// </summary>
        /// <value>Intent name.</value>
        [JsonProperty("name")]
        public string Name { get; }

        /// <summary>
        /// Gets source of the intent.
        /// </summary>
        /// <remarks>This is usually either the id of the recognizer or a reference to the source LU file like foo.lu or MyPackage#foo.lu.</remarks>
        /// <value>Source of the intent definition.</value>
        [JsonProperty("source")]
        public string Source { get; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Name;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return obj is IntentDescription description
                && description.Name == Name
                && description.Source == Source;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
