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
        /// Initializes a new instance of the <see cref="AttachmentPromptInternal"/> class.
        /// </summary>
        /// <param name="validator">The input validator for the prompt object.</param>
        /// <remarks><paramref name="validator"/> is called only if the
        /// <see cref="RecognizeAsync(ITurnContext)"/> method recognizes a value.
        /// </remarks>
        public AttachmentPromptInternal(PromptValidator<AttachmentResult> validator = null)
            : base(validator)
        {
        }

        /// <summary>
        /// Recognizes and validates the user input.
        /// </summary>
        /// <param name="context">Context for the current turn of the conversation with the user.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public override async Task<AttachmentResult> RecognizeAsync(ITurnContext context)
        {
            BotAssert.ContextNotNull(context);
            BotAssert.ActivityNotNull(context.Activity);

            var attachmentResult = new AttachmentResult();
            if (context.Activity.Type == ActivityTypes.Message)
            {
                var message = context.Activity.AsMessageActivity();
                if (message.Attachments != null)
                {
                    attachmentResult.Status = PromptStatus.Recognized;
                    attachmentResult.Attachments.AddRange(message.Attachments);
                    await ValidateAsync(context, attachmentResult);
                }
            }

            return attachmentResult;
        }
    }
}
