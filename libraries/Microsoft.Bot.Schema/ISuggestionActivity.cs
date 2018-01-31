// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace Microsoft.Bot.Schema
{
    /// <summary>
    /// SuggestionActivity (Type="suggestion")
    /// </summary>
    /// <remarks>
    /// A suggestion is a private message for the Recipient which can offer a suggestion activity for an activity by ReplyToId property
    /// </remarks>
    public interface ISuggestionActivity : IMessageActivity
    {
        /// <summary>
        /// TextHighlight in the activity represented in the ReplyToId property
        /// </summary>
        IList<TextHighlight> TextHighlights { get; set; }
    }
}
