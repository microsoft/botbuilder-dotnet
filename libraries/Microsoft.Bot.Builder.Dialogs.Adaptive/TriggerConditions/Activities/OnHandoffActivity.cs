// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions
{
    /// <summary>
    /// Actions triggered when a HandoffActivity is received.
    /// </summary>
    public class OnHandoffActivity : OnActivity
    {
        [JsonProperty("$kind")]
        public new const string Kind = "Microsoft.OnHandoffActivity";

        [JsonConstructor]
        public OnHandoffActivity(List<Dialog> actions = null, string condition = null, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base(type: ActivityTypes.Handoff, actions: actions, condition: condition, callerPath: callerPath, callerLine: callerLine)
        {
        }
    }
}
