using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.DateTime;
using Microsoft.Recognizers.Text.Number;
using Microsoft.Recognizers.Text.NumberWithUnit;
using static Microsoft.Bot.Builder.Prompts.PromptValidatorEx;

namespace Microsoft.Bot.Builder.Prompts
{
    public class ConfirmResult : PromptResult
    {
        public ConfirmResult() { }

        public bool Confirmation { get; set; }

        public string Text { get; set; }
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