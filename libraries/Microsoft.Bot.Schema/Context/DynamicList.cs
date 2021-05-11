// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

// TODO: chrimc, this is lifted from LUIS.  Should we keep in that namespace?  Mark as obsolete?
namespace Microsoft.Bot.Schema
{
    /// <summary>
    /// Defines an extension for a list entity.
    /// </summary>
    public partial class DynamicList
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicList"/> class.
        /// </summary>
        /// <param name="entity">The name of the list entity.</param>
        /// <param name="list">The lists that define the entity.</param>
        [JsonConstructor]
        public DynamicList(string entity, IEnumerable<ListElement> list)
        {
            Entity = entity;
            List = (list == null ? new List<ListElement>() : list.ToList()).AsReadOnly();
        }

        /// <summary>
        /// Gets the name of the list entity to extend.
        /// </summary>
        /// <value>
        /// The name of the list entity to extend.
        /// </value>
        [JsonProperty(PropertyName = "entity")]
        public string Entity { get; }

        /// <summary>
        /// Gets the list of canonical forms and synonyms.
        /// </summary>
        /// <value>
        /// List of canonical forms and synonyms.
        /// </value>
        [JsonProperty(PropertyName = "list")]
        public IReadOnlyList<ListElement> List { get; }
    }
}
