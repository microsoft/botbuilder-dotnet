// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Events
{
    /// <summary>
    /// Event for Invoke Activity.
    /// </summary>
    public class OnInvokeActivity : OnActivity
    {
        [JsonConstructor]
        public OnInvokeActivity(List<IDialog> actions = null, string constraint = null, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base(type: ActivityTypes.Invoke, actions: actions, constraint: constraint, callerPath: callerPath, callerLine: callerLine)
        {
        }
    }
}
