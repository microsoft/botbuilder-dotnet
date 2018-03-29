// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Ai;
using Microsoft.Bot.Builder.BotFramework;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder.LUIS;
using Microsoft.Bot.Schema;
using Microsoft.Cognitive.LUIS;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.Bot.Samples.Ai.Luis.Translator
{
    class CurrentUserState
    {
        public string Language { get; set; }
        public string Locale { get; set; }
    }
    public class Startup
    {
        private static readonly string[] _supportedLanguages = new string[] { "fr", "en" }; //Define supported Languages
        private static readonly string[] _supportedLocales = new string[] { "fr-fr", "en-us" }; //Define supported locales
        private static string currentLanguage = null;
        private static string currentLocale = null;


        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        private void SetLanguage(ITurnContext context, string language) => context.GetConversationState<CurrentUserState>().Language = language;
        private void SetLocale(ITurnContext context, string locale) => context.GetConversationState<CurrentUserState>().Locale = locale;


        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddBot<LuisTranslatorBot>(options =>
            {
            options.CredentialProvider = new ConfigurationCredentialProvider(Configuration);

            string luisModelId = "<Your Model Here>";
            string luisSubscriptionKey = "<Your Key here>";
            Uri luisUri = new Uri("https://westus.api.cognitive.microsoft.com/luis/v2.0/apps/");
            var luisModel = new LuisModel(luisModelId, luisSubscriptionKey, luisUri); 

            // If you want to get all intents scorings, add verbose in luisOptions
            var luisOptions = new LuisRequest { Verbose = true };
            Dictionary<string, List<string>> patterns = new Dictionary<string, List<string>>();
            patterns.Add("fr", new List<string> { "mon nom est (.+)" });//single pattern for fr language
            var middleware = options.Middleware;
            middleware.Add(new ConversationState<CurrentUserState>(new MemoryStorage()));
            middleware.Add(new TranslationMiddleware(new string[] { "en" }, "<your translator key here>", patterns, GetActiveLanguage, SetActiveLanguage));
           middleware.Add(new LocaleConverterMiddleware(GetActiveLocale, SetActiveLocale, "en-us", LocaleConverter.Converter));
            middleware.Add(new LuisRecognizerMiddleware(luisModel, luisOptions: luisOptions));
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseDefaultFiles()
                .UseStaticFiles()
                .UseBotFramework();
        }

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
                if (context.GetConversationState<CurrentUserState>() != null && currentLanguage != context.GetConversationState<CurrentUserState>().Language)
                {
                    SetLanguage(context, currentLanguage);
                }
            }
            if (context.Activity.Type == ActivityTypes.Message
                && context.GetConversationState<CurrentUserState>() != null && context.GetConversationState<CurrentUserState>().Language!=null)
            {
                return context.GetConversationState<CurrentUserState>().Language;
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
                if (context.GetConversationState<CurrentUserState>() != null 
                    && currentLocale != context.GetConversationState<CurrentUserState>().Locale)
                {
                    SetLocale(context, currentLocale);
                }
            }
            if (context.Activity.Type == ActivityTypes.Message
                && context.GetConversationState<CurrentUserState>() != null && context.GetConversationState<CurrentUserState>().Locale!=null)
            {
                return context.GetConversationState<CurrentUserState>().Locale;
            }

            return "en-us";
        }
    }
}
