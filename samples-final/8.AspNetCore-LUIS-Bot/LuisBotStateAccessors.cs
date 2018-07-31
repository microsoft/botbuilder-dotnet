using System.Collections.Generic;
using Microsoft.Bot.Builder;

namespace AspNetCore_LUIS_Bot
{
    public class LuisBotStateAccessors
    {
        public static string DialogStateName = $"{nameof(LuisBotStateAccessors)}.DialogState";
        public static string RemindersName = $"{nameof(LuisBotStateAccessors)}.RemindersState";

        public IStatePropertyAccessor<Dictionary<string, object>> UserDialogState { get; set; }
        public IStatePropertyAccessor<List<Reminder>> Reminders { get; set; }
    }
}
