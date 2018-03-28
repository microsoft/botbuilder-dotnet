// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Ai;
using Microsoft.Bot.Builder.BotFramework;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Core.Extensions;
using System.Collections.Generic;

namespace Microsoft.Bot.Samples.Ai.QnA.Controllers
{
    class CurrentUserState
    {
        public string Language { get; set; }
        public string Locale { get; set; }
    }
    [Route("api/[controller]")]
    public class MessagesController : Controller
    {

        private static readonly HttpClient _httpClient = new HttpClient();
        BotFrameworkAdapter adapter;

        //supported langauges and locales
        private static readonly string[] _supportedLanguages = new string[] { "en", "fr" };
        private static readonly string[] _supportedLocales = new string[] { "fr-fr", "en-us" };
        private static string currentLanguage = null;
        private static string currentLocale = null;


        public MessagesController(IConfiguration configuration)
        {
            if (adapter == null)
            {
                var qnaOptions = new QnAMakerMiddlewareOptions
                {
                    // add subscription key and knowledge base id
                    SubscriptionKey = "xxxxxx",
                    KnowledgeBaseId = "xxxxxx"
                };
                Dictionary<string, List<string>> patterns = new Dictionary<string, List<string>>();
                patterns["fr"].Add("mon nom est (.+)");//single pattern for fr language
                //Check templates forlder for more pattern examples for fr language
                adapter = new BotFrameworkAdapter(new ConfigurationCredentialProvider(configuration))
                    .Use(new UserState<CurrentUserState>(new MemoryStorage()))
                    .Use(new TranslationMiddleware(new string[] { "en" }, "xxxxxx", patterns, GetActiveLanguage, SetActiveLanguage))
                    .Use(new LocaleConverterMiddleware(GetActiveLocale, SetActiveLocale, "en-us", new LocaleConverter()))
                    .Use(new QnAMakerMiddleware(qnaOptions, _httpClient));
            }
        }

        private Task BotReceiveHandler(ITurnContext context)
        {
            if (context.Activity.Type == ActivityTypes.Message && context.Responded == false)
            {
                // add app logic when QnA Maker doesn't find an answer
                context.SendActivity("No good match found in the KB.");
            }
            return Task.CompletedTask;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody]Activity activity)
        {
            try
            {
                await adapter.ProcessActivity(this.Request.Headers["Authorization"].FirstOrDefault(), activity, BotReceiveHandler);
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

        private void SetLanguage(ITurnContext context, string language) => context.GetUserState<CurrentUserState>().Language = language;
        private void SetLocale(ITurnContext context, string locale) => context.GetUserState<CurrentUserState>().Locale = locale;

        protected bool IsSupportedLanguage(string language) => _supportedLanguages.Contains(language);
        protected async Task<bool> SetActiveLanguage(ITurnContext context)
        {
            bool changeLang = false;//logic implemented by developper to make a signal for language changing 
            //use a specific message from user to change language
            var messageActivity = context.Activity.AsMessageActivity();
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
        protected string GetActiveLanguage(ITurnContext context)
        {
            if (currentLanguage != null)
            {
                //user has specified a different language so update the bot state
                if (context.GetUserState<CurrentUserState>() != null && currentLanguage != context.GetUserState<CurrentUserState>().Language)
                {
                    SetLanguage(context, currentLanguage);
                }
            }
            if (context.Activity.Type == ActivityTypes.Message
                && context.GetUserState<CurrentUserState>() != null)
            {
                return (string)context.GetUserState<CurrentUserState>().Language;
            }

            return "en";
        }
        protected async Task<bool> SetActiveLocale(ITurnContext context)
        {
            bool changeLocale = false;//logic implemented by developper to make a signal for language changing 
            //use a specific message from user to change language
            var messageActivity = context.Activity.AsMessageActivity();
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
        protected string GetActiveLocale(ITurnContext context)
        {
            if (currentLocale != null)
            {
                //the user has specified a different locale so update the bot state
                if (context.GetUserState<CurrentUserState>() != null && currentLocale != context.GetUserState<CurrentUserState>().Locale)
                {
                    SetLocale(context, currentLocale);
                }
            }
            if (context.Activity.Type == ActivityTypes.Message
                && context.GetUserState<CurrentUserState>() != null)
            {
                return context.GetUserState<CurrentUserState>().Locale;
            }

            return "en-us";
        }
    }
}
