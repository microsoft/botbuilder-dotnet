// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Bot.Builder.Expressions;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Events
{
    /// <summary>
    /// Rule triggered when a dialog is started via BeginDialog()
    /// </summary>
    public class OnBeginDialog : OnDialogEvent
    {
        [JsonConstructor]
        public OnBeginDialog(List<IDialog> actions = null, string constraint = null, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base(events: new List<string>()
            {
                AdaptiveEvents.BeginDialog
            },
            actions: actions,
            constraint: constraint,
            callerPath: callerPath, callerLine: callerLine)
        {
        }
    }
}
