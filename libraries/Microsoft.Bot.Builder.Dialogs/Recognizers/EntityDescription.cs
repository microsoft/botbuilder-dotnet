// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Recognizers
{
    /// <summary>
    /// Description of an a recognizer entity.
    /// </summary>
    public class EntityDescription
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EntityDescription"/> class.
        /// </summary>
        /// <param name="name">Entity name.</param>
        /// <param name="source">Source of intent definition.</param>
        public EntityDescription(string name, string source = null)
        {
            Name = name;
            Source = source;
        }

        /// <summary>
        /// Gets name of the entity.
        /// </summary>
        /// <value>Entity name.</value>
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
            var description = obj as EntityDescription;
            return description != null && description.Name == Name && description.Source == Source;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
