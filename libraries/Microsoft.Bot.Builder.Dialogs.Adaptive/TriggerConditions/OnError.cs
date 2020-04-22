// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions
{
    /// <summary>
    /// Actions triggered when an error event has been emitted.
    /// </summary>
    public class OnError : OnDialogEvent
    {
        [JsonProperty("$kind")]
        public new const string Kind = "Microsoft.OnError";

        [JsonConstructor]
        public OnError(List<Dialog> actions = null, string condition = null, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base(@event: AdaptiveEvents.Error, actions: actions, condition: condition, callerPath: callerPath, callerLine: callerLine)
        {
        }

        protected override ActionChangeList OnCreateChangeList(ActionContext actionContext, object dialogOptions = null)
        {
            var changeList = base.OnCreateChangeList(actionContext, dialogOptions);

            // For OnError handling we want to replace the old plan with whatever the error plan is, since the old plan blew up.
            changeList.ChangeType = ActionChangeType.ReplaceSequence;
            return changeList;
        }
    }
}
