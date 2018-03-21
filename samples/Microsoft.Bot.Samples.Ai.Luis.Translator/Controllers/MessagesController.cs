
// Copyright(c) Microsoft Corporation.All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Ai;
using Microsoft.Bot.Builder.BotFramework;
using Microsoft.Bot.Builder.LUIS;
using Microsoft.Bot.Schema;
using Microsoft.Cognitive.LUIS;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Bot.Samples.Ai.Luis.Translator
{
    [Route("api/[controller]")]
    public class MessagesController : Controller
    {
        BotFrameworkAdapter _adapter;

        /// <summary>
        /// In this sample Bot, a new instance of the Bot is created by the controller 
        /// on every incoming HTTP request. The bot is constructed using the credentials
        /// found in the config file. Note that no credentials are needed if testing
        /// the bot locally using the emulator. 
        /// </summary>
        /// 

        private static readonly string[] _supportedLanguages = new string[] { "fr", "en" };
        private static readonly string[] _supportedLocales = new string[] { "fr-fr", "en-us" };
        private static string currentLanguage = null;
        private static string currentLocale = null;

        public MessagesController(IConfiguration configuration)
        {
            if (_adapter == null)
            {
                var luisModel = new LuisModel("modelId", "subscriptionKey", new Uri("https://RegionOfYourLuisApp.api.cognitive.microsoft.com/luis/v2.0/apps/"));
                var options = new LuisRequest { Verbose = true }; // If you want to get all intents scorings, add verbose in luisOptions
                                                                  //LuisRequest options = null;
                Dictionary<string, List<string>> patterns = new Dictionary<string, List<string>>();
                patterns["fr"].Add("mon nom est (.+)");//single pattern for fr language
                //Check templates forlder for more pattern examples for fr language
                _adapter = new BotFrameworkAdapter(new ConfigurationCredentialProvider(configuration))
                    .Use(new TranslationMiddleware(new string[] { "en" }, "xxxxxx", patterns, GetActiveLanguage, SetActiveLanguage))
                    .Use(new LocaleConverterMiddleware(GetActiveLocale, SetActiveLocale, "en-us", new LocaleConverter()))
                    .Use(new LuisRecognizerMiddleware(luisModel, luisOptions: options));
            }   
        }

        private Task BotReceiveHandler(IBotContext context)
        {
            if (context.Request.Type == ActivityTypes.Message)
            {
                var luisResult = context.Get<RecognizerResult>(LuisRecognizerMiddleware.LuisRecognizerResultKey);

                if (luisResult != null)
                {
                    (string key, double score) topItem = luisResult.GetTopScoringIntent();
                    context.SendActivity($"The **top intent** was: **'{topItem.key}'**, with score **{topItem.score}**");

                    context.SendActivity($"Detail of intents scorings:");
                    var intentsResult = new List<string>();
                    foreach (var intent in luisResult.Intents)
                    {
                        intentsResult.Add($"* '{intent.Key}', score {intent.Value}");
                    }
                    context.SendActivity(string.Join("\n\n", intentsResult));
                }
            }
            return Task.CompletedTask;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody]Activity activity)
        {
            try
            {
                await _adapter.ProcessActivity(this.Request.Headers["Authorization"].FirstOrDefault(), activity, BotReceiveHandler);
                return this.Ok();
            }
            catch (UnauthorizedAccessException)
            {
                return this.Unauthorized();
            }
        }

        //Change language and locale
        [HttpGet]
        public IActionResult Get(string lang, string locale)
        {
            currentLanguage = lang;
            currentLocale = locale;
            return new ObjectResult("Success!");
        }

        private void SetLanguage(IBotContext context, string language) => context.Set(@"Microsoft.API.translateTo",language);
        private void SetLocale(IBotContext context, string locale) => context.Set(@"LocaleConverterMiddleware.fromLocale",locale);

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
                    await context.SendActivity($@"Changing your language to {newLang}");
                }
                else
                {
                    await context.SendActivity($@"{newLang} is not a supported language.");
                }
                //intercepts message
                return true;
            }

            return false;
        }
        protected string GetActiveLanguage(IBotContext context)
        {
            if (currentLanguage != null)
            {
                //user has specified a different language so update the bot state
                if (context.Has(@"Microsoft.API.translateTo") &&  currentLanguage != (string)context.Get(@"Microsoft.API.translateTo") )
                {
                    SetLanguage(context, currentLanguage);
                }
            }
            if (context.Request.Type == ActivityTypes.Message
                && context.Has(@"Microsoft.API.translateTo"))
            {
                return (string)context.Get(@"Microsoft.API.translateTo");
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
                    await context.SendActivity($@"Changing your language to {newLocale}");
                }
                else
                {
                    await context.SendActivity($@"{newLocale} is not a supported locale.");
                }
                //intercepts message
                return true;
            }

            return false;
        }
        protected string GetActiveLocale(IBotContext context)
        {
            if (currentLocale != null)
            {
                //the user has specified a different locale so update the bot state
                if (currentLocale != (string)context.Get(@"LocaleConverterMiddleware.fromLocale"))
                {
                    SetLocale(context, currentLocale);
                }
            }
            if (context.Request.Type == ActivityTypes.Message
                && context.Has(@"LocaleConverterMiddleware.fromLocale"))
            {
                return (string)context.Get(@"LocaleConverterMiddleware.fromLocale");
            }

            return "en-us";
        }
    }
}
