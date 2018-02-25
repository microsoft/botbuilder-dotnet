// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.


using Microsoft.Bot.Builder;
using System.Collections.Generic;

namespace AlarmBot.Models
{
    public class AlarmConversationState : IStoreItem
    {
        public string eTag { get; set; }

        public ITopic ActiveTopic { get; set; }
    }

    public class AlarmUserState : IStoreItem
    {
        public string eTag { get; set; }

        public IList<Alarm> Alarms { get; set; }
    }
}
