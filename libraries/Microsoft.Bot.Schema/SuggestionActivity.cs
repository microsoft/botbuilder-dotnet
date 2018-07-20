// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.Bot.Schema
{
    /// <summary>
    /// Represents a private suggestion to the <see cref="Activity.Recipient"/> about another activity.
    /// </summary>
    /// <remarks>
    /// The activity's <see cref="Activity.ReplyToId"/> property identifies the activity being referenced.
    /// The activity's <see cref="Activity.Recipient"/> property indicates which user the suggestion is for.
    /// </remarks>
    public class SuggestionActivity : MessageActivity
    {
        /// <summary>
        /// Gets or sets the sections of text in the referenced message to highlight.
        /// </summary>
        [JsonProperty(PropertyName = "textHighlights")]
        public IList<TextHighlight> TextHighlights { get; set; }
    }
}
