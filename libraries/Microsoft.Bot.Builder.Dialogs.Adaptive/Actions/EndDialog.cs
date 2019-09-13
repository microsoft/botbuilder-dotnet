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
    /// Command to end the current dialog, returning the resultProperty as the result of the dialog.
    /// </summary>
    public class EndDialog : DialogAction
    {
        [JsonConstructor]
        public EndDialog(string resultProperty = null, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base()
        {
            this.RegisterSourceLocation(callerPath, callerLine);

            if (!string.IsNullOrEmpty(resultProperty))
            {
                ResultProperty = resultProperty;
            }
        }

        /// <summary>
        /// Gets or sets the property to return as the result ending the dialog.
        /// </summary>
        /// <value>
        /// The property to return as the result ending the dialog.
        /// </value>
        public string ResultProperty { get; set; } = "dialog.result";

        protected override async Task<DialogTurnResult> OnRunCommandAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (options is CancellationToken)
            {
                throw new ArgumentException($"{nameof(options)} cannot be a cancellation token");
            }

            dc.State.TryGetValue<string>(ResultProperty, out var result);
            return await EndParentDialogAsync(dc, result, cancellationToken).ConfigureAwait(false);
        }

        protected override string OnComputeId()
        {
            return $"end({this.ResultProperty ?? string.Empty})";
        }
    }
}
