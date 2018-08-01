// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Microsoft.Bot.Builder;

namespace AspNetCore_LUIS_Bot
{
    /// <summary>
    /// Creates the State Accessors used by the LuisBot. In general usage, this class
    /// is created as a Singleton and passed into the IBot-derived LuisBot constructor.
    ///  - See MyLuisBot.cs constructor for how that is injected 
    ///  - See the Startup.cs file for more details on creating the Singleton that gets
    ///    injected into the constructor.
    /// 
    /// </summary>
    public class MyBotAccessors
    {        
        public static string DialogStateName = $"{nameof(MyBotAccessors)}.DialogState";
        public static string RemindersName = $"{nameof(MyBotAccessors)}.RemindersState";

        public IStatePropertyAccessor<List<Reminder>> Reminders { get; set; }

        public IStatePropertyAccessor<Dictionary<string, object>> UserDialogState { get; set; }     
    }
}
