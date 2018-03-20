// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Microsoft.Bot.Builder.Core.Extensions;

namespace AlarmBot.Models
{
    /// <summary>
    /// Object persisted as conversation state
    /// </summary>
    public class ConversationData : StoreItem
    {
        public ITopic ActiveTopic { get; set; }
    }

    /// <summary>
    /// Object persisted as user state 
    /// </summary>
    public class UserData : StoreItem
    {
        public IList<Alarm> Alarms { get; set; }
    }
}
