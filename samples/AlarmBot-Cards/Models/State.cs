// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Core.Extensions;
using System.Collections.Generic;

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
