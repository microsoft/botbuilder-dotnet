// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Actions
{
    /// <summary>
    /// Action which begins executing another dialog, when it is done, it will return to the caller.
    /// </summary>
    public class BeginDialog : BaseInvokeDialog
    {
        [JsonConstructor]
        public BeginDialog(string dialogIdToCall = null, string property = null, IDictionary<string, string> options = null, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base(dialogIdToCall, property, options)
        {
            this.RegisterSourceLocation(callerPath, callerLine);
        }

        protected async override Task<DialogTurnResult> OnRunCommandAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (options is CancellationToken)
            {
                throw new ArgumentException($"{nameof(options)} cannot be a cancellation token");
            }

            var dialog = this.ResolveDialog(dc);

            // use bindingOptions to bind to the bound options
            var boundOptions = BindOptions(dc, options);

            // start dialog with bound options passed in as the options
            return await dc.BeginDialogAsync(dialog.Id, options: boundOptions, cancellationToken: cancellationToken).ConfigureAwait(false);
        }
    }
}
