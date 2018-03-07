
// Copyright(c) Microsoft Corporation.All rights reserved.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Ai;
using Microsoft.Bot.Builder.BotFramework;
using Microsoft.Bot.Builder.Middleware;
using Microsoft.Bot.Builder.Storage;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Bot.Samples.Ai.Luis.Translator
{
    [Route("api/[controller]")]
    public class MessagesController : Controller
    {
        BotFrameworkAdapter _adapter;

        /// <summary>
        /// In this sample Bot, a new instance of the Bot is created by the controller 
        /// on every incoming HTTP reques. The bot is constructed using the credentials
        /// found in the config file. Note that no credentials are needed if testing
        /// the bot locally using the emulator. 
        /// </summary>
        /// 

        private static readonly string[] _supportedLanguages = new string[] { "fr", "en" };
        private static readonly string[] _supportedLocales = new string[] { "fr-fr", "en-us" };

        public MessagesController(IConfiguration configuration)
        {
            var bot = new Builder.Bot(new BotFrameworkAdapter(configuration))
                .Use(new BotStateManager(new FileStorage(System.IO.Path.GetTempPath()))) //store user state in a temp directory
                .Use(new TranslationMiddleware(new string[] { "en" }, "xxxxxx", "", GetActiveLanguage, SetActiveLanguage))
                .Use(new LocaleConverterMiddleware(GetActiveLocale, SetActiveLocale, "en-us", new LocaleConverter()))
                // add QnA middleware 
                .Use(new LuisRecognizerMiddleware("xxxxxx", "xxxxxx"));
            
          
            
                // LUIS with correct baseUri format example
                //.Use(new LuisRecognizerMiddleware("xxxxxx", "xxxxxx", "https://xxxxxx.api.cognitive.microsoft.com/luis/v2.0/apps"))
                
            bot.OnReceive(BotReceiveHandler);

            _adapter = (BotFrameworkAdapter)bot.Adapter;
        }

        private Task BotReceiveHandler(IBotContext context)
        {
            if (context.Request.Type == ActivityTypes.Message)
            {
                if (context.Responses.Count > 0)
                {
                    return Task.CompletedTask;
                }
                context.Reply($"the top intent was: {context.TopIntent.Name}");

                foreach (var entity in context.TopIntent.Entities)
                {
                    context.Reply($"entity: {entity.ValueAs<string>()}");
                }
            }
            return Task.CompletedTask;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody]Activity activity)
        {
            try
            {
                await _adapter.Receive(this.Request.Headers["Authorization"].FirstOrDefault(), activity);
                return this.Ok();
            }
            catch (UnauthorizedAccessException)
            {
                return this.Unauthorized();
            }
        }

        private void SetLanguage(IBotContext context, string language) => context.State.User[@"Microsoft.API.translateTo"] = language;
        private void SetLocale(IBotContext context, string locale) => context.State.User[@"LocaleConverterMiddleware.fromLocale"] = locale;

        protected bool IsSupportedLanguage(string language) => _supportedLanguages.Contains(language);
        protected async Task<bool> SetActiveLanguage(IBotContext context)
        {
            bool changeLang = false;//logic implemented by developper to make a signal for language changing 
            //use a specific message from user to change language
            var messageActivity = context.Request.AsMessageActivity();
            if (messageActivity.Text.ToLower().StartsWith("set my language to"))
            {
                changeLang = true;
            }
            if (changeLang)
            {
                var newLang = messageActivity.Text.ToLower().Replace("set my language to", "").Trim();
                if (!string.IsNullOrWhiteSpace(newLang)
                        && IsSupportedLanguage(newLang))
                {
                    SetLanguage(context, newLang);
                    context.Reply($@"Changing your language to {newLang}");
                }
                else
                {
                    context.Reply($@"{newLang} is not a supported language.");
                }
                //intercepts message
                return true;
            }

            return false;
        }
        protected string GetActiveLanguage(IBotContext context)
        {
            if (context.Request.Type == ActivityTypes.Message
                && context.State.User.ContainsKey(@"Microsoft.API.translateTo"))
            {
                return (string)context.State.User[@"Microsoft.API.translateTo"];
            }

            return "en";
        }
        protected async Task<bool> SetActiveLocale(IBotContext context)
        {
            bool changeLocale = false;//logic implemented by developper to make a signal for language changing 
            //use a specific message from user to change language
            var messageActivity = context.Request.AsMessageActivity();
            if (messageActivity.Text.ToLower().StartsWith("set my locale to"))
            {
                changeLocale = true;
            }
            if (changeLocale)
            {
                var newLocale = messageActivity.Text.ToLower().Replace("set my locale to", "").Trim(); //extracted by the user using user state 
                if (!string.IsNullOrWhiteSpace(newLocale)
                        && IsSupportedLanguage(newLocale))
                {
                    SetLocale(context, newLocale);
                    context.Reply($@"Changing your language to {newLocale}");
                }
                else
                {
                    context.Reply($@"{newLocale} is not a supported locale.");
                }
                //intercepts message
                return true;
            }

            return false;
        }
        protected string GetActiveLocale(IBotContext context)
        {
            if (context.Request.Type == ActivityTypes.Message
                && context.State.User.ContainsKey(@"LocaleConverterMiddleware.fromLocale"))
            {
                return (string)context.State.User[@"LocaleConverterMiddleware.fromLocale"];
            }

            return "en-us";
        }
    }
}
