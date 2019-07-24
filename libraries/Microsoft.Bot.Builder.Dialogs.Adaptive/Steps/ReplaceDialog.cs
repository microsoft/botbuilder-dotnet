// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Steps
{
    /// <summary>
    /// Step which calls another dialog, when it is done it will go to the callers parent dialog
    /// </summary>
    public class ReplaceDialog : BaseInvokeDialog
    {
        [JsonConstructor]
        public ReplaceDialog(string dialogIdToCall = null, string property = null, object options = null, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
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

            Options = ObjectPath.Merge(Options, options ?? new object());
            BindOptions(dc);

            return await dc.ReplaceDialogAsync(dialog.Id, Options, cancellationToken).ConfigureAwait(false);
        }
    }
}
