// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.


using Microsoft.Bot.Builder;
using System.Collections.Generic;

namespace AlarmBot.Models
{
    public class ConversationState : IStoreItem
    {
        public string eTag { get; set; }

        public ITopic ActiveTopic { get; set; }
    }

    public class UserState : IStoreItem
    {
        public string eTag { get; set; }

        public IList<Alarm> Alarms { get; set; }
    }
}
