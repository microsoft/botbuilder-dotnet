// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Actions
{
    /// <summary>
    /// This command ends the current turn without ending the dialog.
    /// </summary>
    public class EndTurn : DialogAction
    {
        [JsonConstructor]
        public EndTurn([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base()
        {
            this.Id = OnComputeId();
            this.RegisterSourceLocation(callerPath, callerLine);
        }

        protected override string OnComputeId()
        {
            return $"EndTurn[]";
        }

        protected async override Task<DialogTurnResult> OnRunCommandAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (options is CancellationToken)
            {
                throw new ArgumentException($"{nameof(options)} cannot be a cancellation token");
            }

            return await Task.FromResult(Dialog.EndOfTurn);
        }
    }
}
