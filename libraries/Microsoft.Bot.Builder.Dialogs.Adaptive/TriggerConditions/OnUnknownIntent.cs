// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions
{
    /// <summary>
    /// Actions triggered when a UnknownIntent event has been emitted by the recognizer.
    /// </summary>
    /// <remarks>
    /// This trigger is run when the utterance is not recognized and the fallback consultation is happening 
    /// It will only trigger if and when 
    /// * it is the leaf dialog AND 
    /// * none of the parent dialogs handle the event 
    /// This provides the parent dialogs the opportunity to handle global commands as fallback interruption.
    /// </remarks>
    public class OnUnknownIntent : OnDialogEvent
    {
        [JsonProperty("$kind")]
        public new const string Kind = "Microsoft.OnUnknownIntent";

        [JsonConstructor]
        public OnUnknownIntent(List<Dialog> actions = null, string condition = null, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base(
                @event: AdaptiveEvents.UnknownIntent,
                actions: actions,
                condition: condition,
                callerPath: callerPath, 
                callerLine: callerLine)
        {
        }
    }
}
