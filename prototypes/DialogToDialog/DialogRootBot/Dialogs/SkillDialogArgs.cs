// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace DialogRootBot.Dialogs
{
    public class SkillDialogArgs
    {
        public string SkillId { get; set; }

        public string EventName { get; set; }

        public object Value { get; set; }
    }
}
