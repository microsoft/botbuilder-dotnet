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
    /// Send an activity back to the user.
    /// </summary>
    public class SendActivity : DialogCommand
    {
        /// <summary>
        /// Template for the activity.
        /// </summary>
        public ITemplate<Activity> Activity { get; set; }

        [JsonConstructor]
        public SendActivity(string text = null, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
        {
            this.RegisterSourceLocation(callerPath, callerLine);
            this.Activity = new ActivityTemplate(text ?? string.Empty);
        }

        public SendActivity(Activity activity, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
        {
            this.RegisterSourceLocation(callerPath, callerLine);
            this.Activity = new StaticActivityTemplate(activity);
        }

        protected override async Task<DialogTurnResult> OnRunCommandAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (options is CancellationToken)
            {
                throw new ArgumentException($"{nameof(options)} cannot be a cancellation token");
            }

            var activity = await Activity.BindToData(dc.Context, dc.State).ConfigureAwait(false);
            var response = await dc.Context.SendActivityAsync(activity, cancellationToken).ConfigureAwait(false);
            return await dc.EndDialogAsync(response, cancellationToken).ConfigureAwait(false);
        }

        protected override string OnComputeId()
        {
            if (Activity is ActivityTemplate at)
            {
                return $"SendActivity({Ellipsis(at.Template.Trim(), 30)})";
            }

            return $"SendActivity('{Ellipsis(Activity?.ToString().Trim(), 30)}')";
        }

        private static string Ellipsis(string text, int length)
        {
            if (text.Length <= length) return text;
            int pos = text.IndexOf(" ", length);

            if (pos >= 0)
            {
                return text.Substring(0, pos) + "...";
            }

            return text;
        }
    }
}
