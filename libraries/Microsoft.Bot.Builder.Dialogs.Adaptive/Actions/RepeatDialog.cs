// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Actions
{
    public class RepeatDialog : DialogAction
    {
        [JsonConstructor]
        public RepeatDialog([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0) 
            : base()
        {
            this.RegisterSourceLocation(callerPath, callerLine);
        }

        protected override async Task<DialogTurnResult> OnRunCommandAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (options is CancellationToken)
            {
                throw new ArgumentException($"{nameof(options)} cannot be a cancellation token");
            }

            object originalOptions;
            bool originalOptionsFound = dc.State.Dialog.TryGetValue("options", out originalOptions);

            options = options == null ? originalOptions : ObjectPath.Merge(options, originalOptions ?? new object());
            return await RepeatParentDialogAsync(dc, options, cancellationToken).ConfigureAwait(false);
        }

        protected override string OnComputeId()
        {
            return $"RepeatDialog({BindingPath()})";
        }
    }
}
