// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Steps
{
    /// <summary>
    /// Send an activity back to the user
    /// </summary>
    public class SendActivity : DialogCommand
    {
        /// <summary>
        /// Template for the activity
        /// </summary>
        public ITemplate<Activity> Activity { get; set; }

        [JsonConstructor]
        public SendActivity(string text = null, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
        {
            this.RegisterSourceLocation(callerPath, callerLine);
            this.Activity = new ActivityTemplate(text ?? string.Empty);
        }

        protected override async Task<DialogTurnResult> OnRunCommandAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (options is CancellationToken)
            {
                throw new ArgumentException($"{nameof(options)} cannot be a cancellation token");
            }

            var activity = await Activity.BindToData(dc.Context, dc.State, (property, data) =>
            {
                if (data.TryGetValue<object>(property, out object val))
                    return val;
                return null;
            }).ConfigureAwait(false);
            var response = await dc.Context.SendActivityAsync(activity, cancellationToken).ConfigureAwait(false);
            return await dc.EndDialogAsync(response, cancellationToken).ConfigureAwait(false);
        }

        protected override string OnComputeId()
        {
            return $"SendActivity({Activity?.ToString()})";
        }
    }
}
