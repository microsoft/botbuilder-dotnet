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

        protected override async Task<InputState> OnRecognizeInput(DialogContext dc, bool consultation)
        {
            var input = dc.State.GetValue<object>(INPUT_PROPERTY);

            var attachments = new List<Attachment>();
            if (input is List<Attachment>)
            {
                attachments.AddRange((List<Attachment>)input);
            }
            if (input is Attachment)
            {
                attachments.Add((Attachment)input);
            }

            var first = attachments.Count > 0 ? attachments[0] : null;

            if (first == null || (string.IsNullOrEmpty(first.ContentUrl) && first.Content == null))
            {
                return InputState.Unrecognized;
            }

            switch (this.OutputFormat)
            {
                case AttachmentOutputFormat.All:
                    dc.State.SetValue(INPUT_PROPERTY, attachments);
                    break;
                case AttachmentOutputFormat.First:
                    dc.State.SetValue(INPUT_PROPERTY, first);
                    break;
            }

            return InputState.Valid;
        }
    }
}
