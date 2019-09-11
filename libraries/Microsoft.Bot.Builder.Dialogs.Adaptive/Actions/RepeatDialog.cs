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

            object originalOptions = dc.State.GetValue<object>(DialogPath.OPTIONS);

            if (options == null)
            {
                options = originalOptions;
            }
            else if (originalOptions != null)
            {
                options = ObjectPath.Merge(options, originalOptions);
            }

            return await RepeatParentDialogAsync(dc, options, cancellationToken).ConfigureAwait(false);
        }
    }
}
