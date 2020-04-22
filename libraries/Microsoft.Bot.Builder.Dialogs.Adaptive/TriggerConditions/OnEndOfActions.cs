// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions
{
    /// <summary>
    /// Triggered when all actions and ambiguity events have been processed.
    /// </summary>
    public class OnEndOfActions : OnDialogEvent
    {
        [JsonProperty("$kind")]
        public new const string Kind = "Microsoft.OnEndOfActions";
        
        [JsonConstructor]
        public OnEndOfActions(List<Dialog> actions = null, string condition = null, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base(
                @event: AdaptiveEvents.EndOfActions,
                actions: actions,
                condition: condition,
                callerPath: callerPath,
                callerLine: callerLine)
        {
        }
    }
}
