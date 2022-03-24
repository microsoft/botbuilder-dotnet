// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace Microsoft.Bot.Schema
{
    /// <summary>
    /// Defines the structure that arrives in the Activity.Value for Invoke activity
    /// with Name of 'application/search'.
    /// </summary>
    public class SearchInvokeValue
    {
        /// <summary>
        /// Gets or sets the kind for the search invoke value.
        /// Must be either search, searchAnswer, or typeahead.
        /// <see cref="SearchInvokeTypes"/>.
        /// </summary>
        /// <value>
        /// The kind for this search invoke action value.
        /// </value>
        [JsonProperty("kind")]
        public string Kind { get; set; }

        /// <summary>
        /// Gets or sets the query text for the search invoke value.
        /// </summary>
        /// <value>
        /// The query text of this search invoke action value.
        /// </value>
        [JsonProperty("queryText")]
        public string QueryText { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="SearchInvokeOptions"/> for this search invoke.
        /// </summary>
        /// <value>
        /// The <see cref="SearchInvokeOptions"/> for this search invoke.
        /// </value>
        [JsonProperty("queryOptions")]
        public SearchInvokeOptions QueryOptions { get; set; }

        /// <summary>
        /// Gets or sets the context information about the query. Such as the UI
        /// control that issued the query. The type of the context field is object
        /// and is dependent on the kind field. For search and searchAnswers,
        /// there is no defined context value.
        /// </summary>
        /// <value>
        /// The context information about the query.
        /// </value>
        [JsonProperty("context")]
        public object Context { get; set; }
    }
}
