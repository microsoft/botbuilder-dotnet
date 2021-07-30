// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

// TODO: chrimc, this is lifted from LUIS.  Should we keep in that namespace?  Mark as obsolete?
namespace Microsoft.Bot.Schema
{
    /// <summary>
    /// Defines a sub-list to append to an existing list entity.
    /// </summary>
    public partial class ListElement
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
        /// <param name="synonyms">The synonyms of the canonical form.</param>
        public ListElement(string canonicalForm, IEnumerable<string> synonyms = null)
        {
            CanonicalForm = canonicalForm;
            Synonyms = (synonyms == null ? new List<string>() : synonyms.ToList()).AsReadOnly();
        }

        /// <summary>
        /// Gets or sets the canonical form of the sub-list.
        /// </summary>
        /// <value>
        /// The canonical form of the sub-list.
        /// </value>
        [JsonProperty(PropertyName = "canonicalForm")]
        public string CanonicalForm { get; set; }

        /// <summary>
        /// Gets synonyms of the canonical form.
        /// </summary>
        /// <value>
        /// The synonyms of the canonical form.
        /// </value>
        [JsonProperty(PropertyName = "synonyms")]
        public IReadOnlyList<string> Synonyms { get; } 
    }
}
