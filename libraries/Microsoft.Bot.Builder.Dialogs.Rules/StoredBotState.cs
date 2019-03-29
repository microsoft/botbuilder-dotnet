using System.Collections.Generic;

namespace Microsoft.Bot.Builder.Dialogs.Rules
{
    public class StoredBotState
    {
        public Dictionary<string, object> UserState { get; set; }
        public Dictionary<string, object> ConversationState { get; set; }
        public List<DialogInstance> DialogStack { get; set; }
    }
}
