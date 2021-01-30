using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Bot.Builder.Runtime.Settings
{
    internal class RuntimeFeatures
    {
        public bool RemoveRecipientMention { get; set; }

        public bool UseShowTypingMiddleware { get; set; }

        public bool UseInspectionMiddleware { get; set; }
    }
}
