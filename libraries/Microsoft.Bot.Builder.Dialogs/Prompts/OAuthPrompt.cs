using System;
using prompts = Microsoft.Bot.Builder.Prompts;
using static Microsoft.Bot.Builder.Prompts.PromptValidatorEx;
using Microsoft.Bot.Schema;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Prompts;

namespace Microsoft.Bot.Builder.Dialogs
{
    public class OAuthPromptSettingsWithTimeout : prompts.OAuthPromptSettings
    {
        public int? Timeout { get; set; }
    }

    public class OAuthPromptState : PromptOptions
    {
        public OAuthPromptState(PromptOptions defaultPromptOptions) : base()
        {
            if (defaultPromptOptions != null)
            {
                PromptString = PromptString ?? defaultPromptOptions.PromptString;
                PromptActivity = PromptActivity ?? defaultPromptOptions.PromptActivity;
                Speak = Speak ?? defaultPromptOptions.Speak;
                RetryPromptString = RetryPromptString ?? defaultPromptOptions.RetryPromptString;
                RetryPromptActivity = RetryPromptActivity ?? defaultPromptOptions.RetryPromptActivity;
                RetrySpeak = RetrySpeak ?? defaultPromptOptions.RetrySpeak;
            }
        }

        public DateTime Expires { get; set; }
    }

    public class OAuthPrompt : Control, IDialog, IDialogContinue
    {
        private prompts.OAuthPrompt _prompt;
        private OAuthPromptSettingsWithTimeout _settings;

        public OAuthPrompt(OAuthPromptSettingsWithTimeout settings, PromptValidator<TokenResult> validator = null)
        {
            _settings = settings ?? throw new ArgumentException(nameof(settings));
            _prompt = new prompts.OAuthPrompt(settings, validator);
        }

        public async Task DialogBegin(DialogContext dc, object dialogArgs = null)
        {
            if (dc == null)
                throw new ArgumentNullException(nameof(dc));
            if (dialogArgs == null)
                throw new ArgumentNullException(nameof(dialogArgs));

            var promptOptions = (PromptOptions)dialogArgs;

            //persist options and state
            var timeout = _settings.Timeout.HasValue ? _settings.Timeout.Value : 54000000;
            var instance = dc.Instance;
            instance.State = new OAuthPromptState(promptOptions);

            var tokenResult = await _prompt.GetUserToken(dc.Context);

            if (tokenResult != null && tokenResult.Value != null)
            {
                await dc.End(tokenResult);
            }
            else if (!string.IsNullOrEmpty(promptOptions.PromptString))
            {
                await _prompt.Prompt(dc.Context, promptOptions.PromptString, promptOptions.Speak);
            }
            else if (promptOptions.PromptActivity != null)
            {
                await _prompt.Prompt(dc.Context, promptOptions.PromptActivity);
            }
        }

        public async Task DialogContinue(DialogContext dc)
        {
            if (dc == null)
                throw new ArgumentNullException(nameof(dc));
            //Recognize token
            var tokenResult = await _prompt.Recognize(dc.Context);
            //Check for timeout
            var state = dc.Instance.State as OAuthPromptState;
            var isMessage = dc.Context.Activity.Type == ActivityTypes.Message;
            var hasTimedOut = isMessage && (DateTime.Compare(DateTime.Now, state.Expires) > 0);

            if (tokenResult == null || hasTimedOut)
            {
                await dc.End(tokenResult);
            }
            else if (isMessage && !string.IsNullOrEmpty(state.RetryPromptString))
            {
                await dc.Context.SendActivity(state.RetryPromptString, state.RetrySpeak);
            }
        }
    }
}