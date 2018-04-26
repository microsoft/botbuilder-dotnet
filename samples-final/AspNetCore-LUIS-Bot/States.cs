using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;

namespace AspNetCore_LUIS_Bot
{ 
    public class Reminder : WaterfallInstance
    {
        public string Title { get; set; }
        public DateTime? Date { get; set; }
    }

    public class UserState : Dictionary<string, object>
    {
        public UserState()
        {
            this[nameof(Reminders)] = new List<Reminder>();
        }

        public IList<Reminder> Reminders
        {
            get { return this[nameof(Reminders)] as IList<Reminder>; }
            set { this[nameof(Reminders)] = value; }
        }
    }
}
