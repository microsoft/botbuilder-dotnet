using System.Collections.Generic;

namespace Microsoft.Bot.Builder.Dialogs.Rules
{
    public class PlanningDialogRunOptions
    {
        public BotState BotState { get; set; }

        public object DialogOptions { get; set; }

        public int? ExpireAfter { get; set; }

        public Dictionary<string, object> UserState { get; set; }
    }
}
