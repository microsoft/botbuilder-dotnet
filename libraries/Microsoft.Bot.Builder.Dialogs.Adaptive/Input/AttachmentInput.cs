// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Input
{
    /// <summary>
    /// Format specifier for outputs.
    /// </summary>
    public enum AttachmentOutputFormat
    {
        /// <summary>
        /// Pass iputs in a List.
        /// </summary>
        All,

        /// <summary>
        /// Pass input as a single element.
        /// </summary>
        First
    }

    public class AttachmentInput : InputDialog
    {
        public AttachmentInput([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
        {
            this.RegisterSourceLocation(callerPath, callerLine);
        }

        public AttachmentOutputFormat OutputFormat { get; set; } = AttachmentOutputFormat.First;

        protected override string OnComputeId()
        {
            return $"AttachmentInput[{BindingPath()}]";
        }

        protected override Task<InputState> OnRecognizeInput(DialogContext dc)
        {
            var input = dc.State.GetValue<List<Attachment>>(InputProperty);
            var first = input.Count > 0 ? input[0] : null;

            if (first == null || (string.IsNullOrEmpty(first.ContentUrl) && first.Content == null))
            {
                return Task.FromResult(InputState.Unrecognized);
            }

            switch (this.OutputFormat)
            {
                case AttachmentOutputFormat.All:
                    dc.State.SetValue(InputProperty, input);
                    break;
                case AttachmentOutputFormat.First:
                    dc.State.SetValue(InputProperty, first);
                    break;
            }

            return Task.FromResult(InputState.Valid);
        }
    }
}
