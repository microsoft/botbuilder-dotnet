// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System.Collections.Generic;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive
{
    public class PlanningDialogRunOptions
    {
        public BotState BotState { get; set; }

        public object DialogOptions { get; set; }

        public int? ExpireAfter { get; set; }

        public IDictionary<string, object> UserState { get; set; }
    }
}
