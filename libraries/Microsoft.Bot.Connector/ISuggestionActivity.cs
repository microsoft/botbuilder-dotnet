using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Bot.Connector
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
