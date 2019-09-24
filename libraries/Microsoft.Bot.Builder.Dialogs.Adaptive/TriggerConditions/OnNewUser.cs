// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Bot.Builder.Expressions;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions
{
    /// <summary>
    ///  Actions triggered when a new user is deteceted
    /// </summary>
    public class OnNewUser : OnDialogEvent
    {
        [JsonConstructor]
        public OnNewUser(List<Dialog> actions = null, string condition = null, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base(@event: AdaptiveEvents.NewUser, condition: condition, actions: actions, callerPath: callerPath, callerLine: callerLine)
        {
        }
    }
}
