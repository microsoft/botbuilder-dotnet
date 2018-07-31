// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Microsoft.Bot.Builder;

namespace AspNetCore_LUIS_Bot
{
    /// <summary>
    /// Creates the State Accessors used by the LuisBot. In general usage, this class
    /// is created as a Singleton and passed into the LuisBot constructor
    /// </summary>
    public class LuisBotStateAccessors
    {        
        public static string DialogStateName = $"{nameof(LuisBotStateAccessors)}.DialogState";
        public static string RemindersName = $"{nameof(LuisBotStateAccessors)}.RemindersState";

        public IStatePropertyAccessor<List<Reminder>> Reminders { get; set; }

        public IStatePropertyAccessor<Dictionary<string, object>> UserDialogState { get; set; }     
    }
}
