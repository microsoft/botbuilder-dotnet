// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Events
{
    /// <summary>
    /// This rule fires when the utterance is not recognized and the fallback consultation is happening 
    /// It will only trigger if and when 
    /// * it is the leaf dialog AND 
    /// * none of the parent dialogs handle the event 
    /// This provides the parent dialogs the opportunity to handle global commands as fallback interruption.
    /// </summary>
    public class OnUnknownIntent : OnDialogEvent
    {
        [JsonConstructor]
        public OnUnknownIntent(List<IDialog> actions = null, string constraint = null, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base(
                events: new List<string>()
                {
                    AdaptiveEvents.UnknownIntent
                },
                actions: actions,
                constraint: constraint,
                callerPath: callerPath, 
                callerLine: callerLine)
            {
        }
    }
}
