// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs
{
    public class AttachmentPrompt : Prompt<AttachmentResult>
    {
        private AttachmentPromptInternal _prompt;

        public AttachmentPrompt()
        {
            _prompt = new AttachmentPromptInternal();
        }

        protected override Task OnPrompt(DialogContext dc, PromptOptions options, bool isRetry)
        {
            if (dc == null)
                throw new ArgumentNullException(nameof(dc));
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            return dc.Context.SendActivityAsync(PromptMessageFactory.CreateActivity(options, isRetry));
        }

        protected override async Task<AttachmentResult> OnRecognize(DialogContext dc, PromptOptions options)
        {
            if (dc == null)
                throw new ArgumentNullException(nameof(dc));
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            return await _prompt.Recognize(dc.Context);
        }
    }
}
