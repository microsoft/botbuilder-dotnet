// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder.Ai;
using Microsoft.Bot.Builder.BotFramework;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Builder.Core.State;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder.Ai.LUIS;
using Microsoft.Bot.Builder.Ai.Translation;
using Microsoft.Cognitive.LUIS;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.Bot.Samples.Ai.Luis.Translator
{

    public class Startup
    { 
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
            middleware.Add(new StateManagementMiddleware()
                            .UseDefaultStorageProvider(new MemoryStateStorageProvider())
                            .UseConversationState());
            middleware.Add(new TranslationMiddleware(new string[] { "en" }, "<your translator key here>", patterns, TranslatorLocaleHelper.GetActiveLanguage, TranslatorLocaleHelper.CheckUserChangedLanguage));
            middleware.Add(new LocaleConverterMiddleware(TranslatorLocaleHelper.GetActiveLocale, TranslatorLocaleHelper.CheckUserChangedLocale, "en-us", LocaleConverter.Converter));
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

    }
}
