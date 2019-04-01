// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Rules
{
    /// <summary>
    /// This rule fires when no other rule has fired. This allows you to set up a steps for "falling back"
    /// </summary>
    public class NoMatchRule : EventRule
    {
        public NoMatchRule(List<IDialog> steps = null)
            : base(new List<string>() { PlanningEvents.Fallback.ToString() }, steps)
        {
        }
    }
}
