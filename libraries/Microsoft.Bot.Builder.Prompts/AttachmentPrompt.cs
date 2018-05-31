// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Schema;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Microsoft.Bot.Builder.Prompts.PromptValidatorEx;

namespace Microsoft.Bot.Builder.Prompts
{
    /// <summary>
    /// Represents recognition result for the prompt.
    /// </summary>
    public class AttachmentResult : PromptResult
    {
        public AttachmentResult()
        {
            Attachments = new List<Attachment>();
        }

        /// <summary>
        /// The collection of attachments recognized
        /// </summary>
        public List<Attachment> Attachments
        {
            get { return GetProperty<List<Attachment>>(nameof(Attachments)); }
            set { this[nameof(Attachments)] = value; }
        }
    }

    /// <summary>
    /// Represents a user prompt class for attachment input.
    /// </summary>
    public class AttachmentPrompt : BasePrompt<AttachmentResult>
    {
        /// <summary>
        /// Creates a <see cref="BasePrompt{T}"/> object.
        /// </summary>
        /// <param name="validator">The input validator for the prompt object.</param>
        /// <remarks><paramref name="validator"/> is called only if the
        /// <see cref="Recognize(ITurnContext)"/> method recognizes a value.
        /// </remarks>
        public AttachmentPrompt(PromptValidator<AttachmentResult> validator = null)
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
            if (context.Activity.Type == ActivityTypes.Message)
            {
                IMessageActivity message = context.Activity.AsMessageActivity();
                if (message.Attachments != null)
                {
                    attachmentResult.Status = PromptStatus.Recognized;
                    attachmentResult.Attachments.AddRange(message.Attachments);
                    await Validate(context, attachmentResult);
                }
            }
            return attachmentResult;
        }
    }
}