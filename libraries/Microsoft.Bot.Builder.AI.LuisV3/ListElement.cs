// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.AI.Luis
{
    /// <summary>
    /// Defines a sub-list to append to an existing list entity.
    /// </summary>
    public class ListElement
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ListElement"/> class.
        /// </summary>
        public ListElement()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ListElement"/> class.
        /// </summary>
        /// <param name="canonicalForm">The canonical form of the sub-list.</param>
        /// <param name="name">The name of the sub-list.</param>
        /// <param name="synonyms">The synonyms of the canonical form.</param>
        public ListElement(string canonicalForm, string name = null, IList<string> synonyms = null)
        {
            CanonicalForm = canonicalForm;
            Name = name;
            Synonyms = synonyms;
        }

        /// <summary>
        /// Gets or sets the name of the sub-list.
        /// </summary>
        /// <value>
        /// The name of the sub-list.
        /// </value>
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the canonical form of the sub-list.
        /// </summary>
        /// <value>
        /// The canonical form of the sub-list.
        /// </value>
        [JsonProperty(PropertyName = "canonicalForm")]
        public string CanonicalForm { get; set; }

        /// <summary>
        /// Gets or sets the synonyms of the canonical form.
        /// </summary>
        /// <value>
        /// The synonyms of the canonical form.
        /// </value>
        [JsonProperty(PropertyName = "synonyms")]
        public IList<string> Synonyms { get; set; }

        /// <summary>
        /// Validate the object.
        /// </summary>
        /// <exception cref="Microsoft.Rest.ValidationException">
        /// Thrown if parameters are invalid.
        /// </exception>
        public virtual void Validate()
        {
            if (CanonicalForm == null || Synonyms == null)
            {
                throw new Microsoft.Rest.ValidationException($"RequestList requires CanonicalForm and Synonyms to be defined.");
            }
        }
    }
}
