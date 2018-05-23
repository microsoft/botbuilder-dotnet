// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.Choice;
using static Microsoft.Bot.Builder.Prompts.PromptValidatorEx;

namespace Microsoft.Bot.Builder.Prompts
{
    public class ConfirmResult : PromptResult
    {
        /// <summary>
        /// The input bool recognized; or <c>null</c>, if recognition fails.
        /// </summary>
        public bool Confirmation
        {
            get { return GetProperty<bool>(nameof(Confirmation)); }
            set { this[nameof(Confirmation)] = value; }
        }

        /// <summary>
        /// The input text recognized; or <c>null</c>, if recognition fails.
        /// </summary>
        public string Text
        {
            get { return GetProperty<string>(nameof(Text)); }
            set { this[nameof(Text)] = value; }
        }
    }

    /// <summary>
    /// ConfirmPrompt recognizes confrimation expressions as bool 
    /// </summary>
    public class ConfirmPrompt : BasePrompt<ConfirmResult>
    {
        private readonly IModel model;

        public ConfirmPrompt(string culture, PromptValidator<ConfirmResult> validator = null)
            : base(validator)
        {
            model = new ChoiceRecognizer(culture).GetBooleanModel(culture);
        }

        public override async Task<ConfirmResult> Recognize(ITurnContext context)
        {
            BotAssert.ContextNotNull(context);
            BotAssert.ActivityNotNull(context.Activity);
            if (context.Activity.Type != ActivityTypes.Message)
                throw new InvalidOperationException("No Message to Recognize");

            IMessageActivity message = context.Activity.AsMessageActivity();
            var confirmResult = new ConfirmResult();
            var results = model.Parse(message.Text);
            if (results.Any())
            {
                var result = results.First();
                if (bool.TryParse(result.Resolution["value"].ToString(), out bool value))
                {
                    confirmResult.Status = PromptStatus.Recognized;
                    confirmResult.Confirmation = value;
                    confirmResult.Text = result.Text;
                    await Validate(context, confirmResult);
                }
            }
            return confirmResult;
        }
    }
}