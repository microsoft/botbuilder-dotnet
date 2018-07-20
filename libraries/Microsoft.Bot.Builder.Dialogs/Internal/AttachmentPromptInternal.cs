// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using static Microsoft.Bot.Builder.Dialogs.PromptValidatorEx;

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// Represents a user prompt class for attachment input.
    /// </summary>
    internal class AttachmentPromptInternal : BasePromptInternal<AttachmentResult>
    {
        /// <summary>
        /// Creates a <see cref="AttachmentPromptInternal"/> object.
        /// </summary>
        /// <param name="validator">The input validator for the prompt object.</param>
        /// <remarks><paramref name="validator"/> is called only if the
        /// <see cref="Recognize(ITurnContext)"/> method recognizes a value.
        /// </remarks>
        public AttachmentPromptInternal(PromptValidator<AttachmentResult> validator = null)
            : base(validator)
        {
        }

        /// <summary>
        /// Recognizes and validates the user input.
        /// </summary>
        public override async Task<AttachmentResult> Recognize(ITurnContext context)
        {
            BotAssert.ContextNotNull(context);
            BotAssert.ActivityNotNull(context.Activity);

            var attachmentResult = new AttachmentResult();
            if (context.Activity is MessageActivity messageActivity)
            {
                if (messageActivity.Attachments != null)
                {
                    attachmentResult.Status = PromptStatus.Recognized;
                    attachmentResult.Attachments.AddRange(messageActivity.Attachments);
                    await Validate(context, attachmentResult);
                }
            }
            return attachmentResult;
        }
    }
}
