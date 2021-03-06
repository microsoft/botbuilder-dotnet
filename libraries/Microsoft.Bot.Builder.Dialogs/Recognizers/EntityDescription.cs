// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System;

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
        public EntityDescription(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Gets name of the entity.
        /// </summary>
        /// <value>Entity name.</value>
        public string Name { get; }

        // TODO: chrimc, not sure we really need this.

        /// <summary>
        /// Gets the type of the resolution for the entity.
        /// </summary>
        /// <value>Type description for entity resolution.</value>
        public Type ResolutionType { get; }
    }
}
