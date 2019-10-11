// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Microsoft.Bot.Schema;

namespace DialogRootBot.Dialogs
{
    public class SkillDialogArgs
    {
        public string TargetAction { get; set; }

        public Dictionary<string, object> Entities { get; } = new Dictionary<string, object>();
    }
}
