// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Actions
{
    /// <summary>
    /// Write log activity to console log.
    /// </summary>
    public class LogAction : DialogAction
    {
        [JsonConstructor]
        public LogAction(string text = null, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
        {
            this.RegisterSourceLocation(callerPath, callerLine);
            if (text != null)
            {
                Text = new TextTemplate(text);
            }
        }

        /// <summary>
        /// Gets or sets lG expression to log.
        /// </summary>
        /// <value>
        /// LG expression to log.
        /// </value>
        [JsonProperty("text")]
        public ITextTemplate Text { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a TraceActivity will be sent in addition to console log.
        /// </summary>
        /// <value>
        /// Whether a TraceActivity will be sent in addition to console log.
        /// </value>
        [JsonProperty("traceActivity")]
        public bool TraceActivity { get; set; } = false;

        protected override async Task<DialogTurnResult> OnRunCommandAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var text = await Text.BindToData(dc.Context, dc.State).ConfigureAwait(false);

            System.Diagnostics.Trace.TraceInformation(text);

            if (this.TraceActivity)
            {
                var traceActivity = Activity.CreateTraceActivity("Log", "Text", text);
                await dc.Context.SendActivityAsync(traceActivity, cancellationToken).ConfigureAwait(false);
            }

            return await dc.EndDialogAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        protected override string OnComputeId()
        {
            return $"LogAction({Text?.ToString()})";
        }
    }
}
