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
    /// This command ends the current turn without ending the dialog
    /// </summary>
    public class EndTurn : Dialog
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

        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (options is CancellationToken)
            {
                throw new ArgumentException($"{nameof(options)} cannot be a cancellation token");
            }


            return Dialog.EndOfTurn;
        }

        public override async Task<DialogConsultation> ConsultDialogAsync(DialogContext dc, CancellationToken cancellationToken = default(CancellationToken))
        {
            return new DialogConsultation()
            {
                Desire = DialogConsultationDesire.CanProcess,
                Processor = async (ctx) =>
                {
                    var activity = ctx.Context.Activity;

                    if (activity.Type == ActivityTypes.Message)
                    {
                        return await ctx.EndDialogAsync(activity.Text, cancellationToken).ConfigureAwait(false);
                    }
                    else
                    {
                        return Dialog.EndOfTurn;
                    }
                }
            };
        }
    }
}
