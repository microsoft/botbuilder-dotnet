// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System; 
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Bot.Schema; 
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
        // private IModel _model;
        private Regex yes = new Regex(@"(\byes\b|\byep\b|\bok\b|\byessir\b|\bconfirm\b|^y$)", RegexOptions.IgnoreCase);
        private Regex no = new Regex(@"(\bno\b|\bnope\b|\bnosir\b|\bcancel\b|^n$)", RegexOptions.IgnoreCase);

        public ConfirmPrompt(string culture, PromptValidator<ConfirmResult> validator = null)
            : base(validator)
        {

        }

        //protected ConfirmPrompt(IModel model, PromptValidator<ConfirmResult> validator = null)
        //    : base(validator)
        //{
        //    this._model = model;
        //}

        public override async Task<ConfirmResult> Recognize(ITurnContext context)
        {
            BotAssert.ContextNotNull(context);
            BotAssert.ActivityNotNull(context.Activity);
            if (context.Activity.Type != ActivityTypes.Message)
                throw new InvalidOperationException("No Message to Recognize");

            IMessageActivity message = context.Activity.AsMessageActivity();
            var confirmResult = new ConfirmResult();
            Match yesMatch = yes.Match(message.Text);
            Match noMatch = no.Match(message.Text);
            if (yesMatch.Success)
            {
                confirmResult.Status = PromptStatus.Recognized;
                confirmResult.Confirmation = true;
                confirmResult.Text = yesMatch.Value;
                await Validate(context, confirmResult);
            }
            else if (noMatch.Success)
            {
                confirmResult.Status = PromptStatus.Recognized;
                confirmResult.Confirmation = false;
                confirmResult.Text = noMatch.Value;
                await Validate(context, confirmResult);
            }
            return confirmResult;
        }
    }
}