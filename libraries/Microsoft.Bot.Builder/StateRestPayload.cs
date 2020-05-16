using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Bot.Builder
{
    public class StateRestPayload
    {
        public string ConversationId { get; set; }

        public string PropertyName { get; set; }

        public IDictionary<string, object> Data { get; set; }
    }
}
