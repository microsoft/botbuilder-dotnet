using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Samples.Dialog.Prompts
{
    public class OAuthPromptBot : IBot
    {
        // Change to point to your connection setting name
        private const string _connectionSettingName = "";
        private DialogSet _dialogs;

        public OAuthPromptBot()
        {
            _dialogs = new DialogSet();
            _dialogs.Add("loginPrompt",
                new OAuthPrompt(
                    new OAuthPromptSettingsWithTimeout()
                    {
                        ConnectionName = _connectionSettingName,
                        Text = "Please Sign In",
                        Title = "Sign In",
                        Timeout = 300000 // User has 5 minutes to login
                    }));
            _dialogs.Add("displayToken",
                new WaterfallStep[]
                {
                    async (dc, args, next) =>
                    {
                        await dc.Begin("loginPrompt");
                    },
                    async (dc, args, next) =>
                    {
                        object token = null;
                        if(args != null && args.TryGetValue("TokenResponse", out token))
                        {
                            //continue with task needing access token
                            await dc.Context.SendActivity($"your token is: {((TokenResponse)token).Token}");
                        }
                        else
                        {
                            await dc.Context.SendActivity("sorry... We couldn't log you in. Try again later.");
                            await dc.End();
                        }
                    }
                });
        }

        public async Task OnTurn(ITurnContext turnContext)
        {
            try
            {
                Dictionary<string, object> state = null;
                DialogContext dc = null;
                switch (turnContext.Activity.Type)
                {
                    case ActivityTypes.Message:
                        if (turnContext.Activity.Text == "signout")
                        {
                            var botframeworkAdapter = turnContext.Adapter as BotFrameworkAdapter;
                            await botframeworkAdapter.SignOutUser(turnContext, _connectionSettingName);
                            await turnContext.SendActivity("You are now signed out.");
                        }
                        else
                        {
                            state = ConversationState<Dictionary<string, object>>.Get(turnContext);
                            dc = _dialogs.CreateContext(turnContext, state);
                            await dc.Continue();

                            // Check to see if anyone replied. If not then start dialog
                            if (!turnContext.Responded)
                            {
                                await dc.Begin("displayToken");
                            }
                        }
                        break;
                    case ActivityTypes.Event:
                    case ActivityTypes.Invoke:
                        // Create dialog context and continue executing the "current" dialog, if any.
                        // This is important for OAuthCards because tokens can be received via TokenResponse events
                        state = ConversationState<Dictionary<string, object>>.Get(turnContext);
                        dc = _dialogs.CreateContext(turnContext, state);
                        await dc.Continue();
                        break;
                }
            }
            catch (Exception e)
            {
                await turnContext.SendActivity($"Exception: {e.Message}");
            }
        }
    }
}
