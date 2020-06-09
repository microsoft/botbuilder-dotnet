using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Microsoft.Bot.Builder.Adapters.Slack.Model.Composition
{
    public enum TextObjectType
    {
        /// <summary>
        /// Plain Text
        /// </summary>
        [EnumMember(Value = "plain_text")]
        PlainText,

        /// <summary>
        /// Markdown
        /// </summary>
        [EnumMember(Value = "mrkdwn")]
        Markdown
    }
}
