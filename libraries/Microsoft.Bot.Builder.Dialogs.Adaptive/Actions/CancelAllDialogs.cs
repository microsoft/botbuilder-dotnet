// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveExpressions.Properties;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Actions
{
    /// <summary>
    /// Command to cancel all of the current dialogs by emitting an event which must be caught to prevent cancelation from propagating.
    /// </summary>
    public class CancelAllDialogs : CancelAllDialogsBase
    {
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.CancelAllDialogs";

        [JsonConstructor]
        public CancelAllDialogs([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base(cancelAll: true)
        {
            this.RegisterSourceLocation(callerPath, callerLine);
        }
    }
}
