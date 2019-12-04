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
    public class RepeatDialog : BaseInvokeDialog
    {
        [JsonProperty("$kind")]
        public const string DeclarativeType = "Microsoft.RepeatDialog";

        [JsonConstructor]
        public RepeatDialog([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base()
        {
            this.RegisterSourceLocation(callerPath, callerLine);
        }

        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (options is CancellationToken)
            {
                throw new ArgumentException($"{nameof(options)} cannot be a cancellation token");
            }

            // use bindingOptions to bind to the bound options
            var boundOptions = BindOptions(dc, options);

            var targetDialogId = dc.Parent.ActiveDialog.Id;

            var repeatedIds = dc.GetState().GetValue<List<string>>(TurnPath.REPEATEDIDS, () => new List<string>());
            if (repeatedIds.Contains(targetDialogId))
            {
                throw new ArgumentException($"Recursive loop detected, {targetDialogId} cannot be repeated twice in one turn.");
            }

            repeatedIds.Add(targetDialogId);
            dc.GetState().SetValue(TurnPath.REPEATEDIDS, repeatedIds);

            if (this.IncludeActivity)
            {
                // reset this to false so that new dialog has opportunity to process the activity
                dc.GetState().SetValue(TurnPath.ACTIVITYPROCESSED, false);
            }

            var turnResult = await dc.Parent.ReplaceDialogAsync(dc.Parent.ActiveDialog.Id, boundOptions, cancellationToken).ConfigureAwait(false);
            turnResult.ParentEnded = true;
            return turnResult;
        }
    }
}
