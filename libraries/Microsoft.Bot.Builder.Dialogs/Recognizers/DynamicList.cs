// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using Newtonsoft.Json;

// TODO: chrimc, this is lifted from LUIS.  Should we keep in that namespace?  Mark as obsolete?
namespace Microsoft.Bot.Builder.Dialogs.Recognizers
{
    /// <summary>
    /// Defines an extension for a list entity.
    /// </summary>
    public class DynamicList
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicList"/> class.
        /// </summary>
        public DynamicList()
        {
        }

        /// <summary>
        /// Gets or sets the name of the list entity to extend.
        /// </summary>
        /// <value>
        /// The name of the list entity to extend.
        /// </value>
        [JsonProperty(PropertyName = "entity")]
        public string Entity { get; set; }

        /// <summary>
        /// Gets the list of canonical forms and synonyms.
        /// </summary>
        /// <value>
        /// List of canonical forms and synonyms.
        /// </value>
        [JsonProperty(PropertyName = "list")]
        public IList<ListElement> List { get; } = new List<ListElement>();
    }
}
