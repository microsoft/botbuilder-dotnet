// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.TriggerHandlers
{
    /// <summary>
    /// Rule triggered when a dialog is started via BeginDialog().
    /// </summary>
    public class OnBeginDialog : OnDialogEvent
    {
        [JsonConstructor]
        public OnBeginDialog(List<Dialog> actions = null, string constraint = null, int priority = 0, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base(
                events: new List<string>()
                {
                    AdaptiveEvents.BeginDialog
                },
                actions: actions,
                constraint: constraint,
                priority: priority,
                callerPath: callerPath, 
                callerLine: callerLine)
        {
        }
    }
}
