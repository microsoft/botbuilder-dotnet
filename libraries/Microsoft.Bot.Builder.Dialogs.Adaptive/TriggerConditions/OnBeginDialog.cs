// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions
{
    /// <summary>
    /// Actions triggered when a dialog is started via BeginDialog().
    /// </summary>
    public class OnBeginDialog : OnDialogEvent
    {
        [JsonProperty("$kind")]
        public new const string Kind = "Microsoft.OnBeginDialog";

        [JsonConstructor]
        public OnBeginDialog(List<Dialog> actions = null, string condition = null, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base(@event: AdaptiveEvents.BeginDialog, actions: actions, condition: condition, callerPath: callerPath, callerLine: callerLine)
        {
        }
    }
}
