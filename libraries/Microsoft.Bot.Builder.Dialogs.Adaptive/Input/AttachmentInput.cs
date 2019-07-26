// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Input
{
    public enum AttachmentOutputFormat
    {
        All,
        First
    }

    public class AttachmentInput : InputDialog
    {
        public AttachmentOutputFormat OutputFormat { get; set; } = AttachmentOutputFormat.First;

        public AttachmentInput([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
        {
            this.RegisterSourceLocation(callerPath, callerLine);
        }

        protected override string OnComputeId()
        {
            return $"AttachmentInput[{BindingPath()}]";
        }

        protected override Task<InputState> OnRecognizeInput(DialogContext dc)
        {
            var input = dc.State.GetValue<List<Attachment>>(INPUT_PROPERTY);
            var first = input.Count > 0 ? input[0] : null;

            if (first == null || (string.IsNullOrEmpty(first.ContentUrl) && first.Content == null))
            {
                return Task.FromResult(InputState.Unrecognized);
            }

            switch (this.OutputFormat)
            {
                case AttachmentOutputFormat.All:
                    dc.State.SetValue(INPUT_PROPERTY, input);
                    break;
                case AttachmentOutputFormat.First:
                    dc.State.SetValue(INPUT_PROPERTY, first);
                    break;
            }

            return Task.FromResult(InputState.Valid);
        }
    }
}
