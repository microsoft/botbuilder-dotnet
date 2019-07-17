// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Actions
{
    /// <summary>
    /// Command to cancel all of the current dialogs by emitting an event which must be caught to prevent cancelation from propagating.
    /// </summary>
    public class CancelAllDialogs : DialogAction
    {
        [JsonConstructor]
        public CancelAllDialogs([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base()
        {
            this.RegisterSourceLocation(callerPath, callerLine);
        }

        /// <summary>
        /// Event name 
        /// </summary>
        public string EventName { get; set; }

        /// <summary>
        /// Event value
        /// </summary>
        public string EventValue { get; set; }

        protected override async Task<DialogTurnResult> OnRunCommandAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (options is CancellationToken)
            {
                throw new ArgumentException($"{nameof(options)} cannot be a cancellation token");
            }

            return await CancelAllParentDialogsAsync(dc, eventName: EventName ?? "cancelDialog", eventValue: EventValue, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        protected override string OnComputeId()
        {
            return "CancelDialog()";
        }
    }
}
