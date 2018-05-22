// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder.BotFramework;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using Microsoft.Bot.Builder.Ai.QnA;
using Microsoft.Bot.Builder.Ai.Translation;

namespace Microsoft.Bot.Samples.Ai.QnA.Translator
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
            services.AddBot<QnAMakerBot>(options =>
            {
                options.CredentialProvider = new ConfigurationCredentialProvider(Configuration);

                var qnaEndpoint = new QnAMakerEndpoint
                {
                    EndpointKey = "xxxxxx",
                    KnowledgeBaseId = "xxxxxx",
                };

                var middleware = options.Middleware;
                Dictionary<string, List<string>> patterns = new Dictionary<string, List<string>>();
                patterns.Add("fr", new List<string> { "mon nom est (.+)" });//single pattern for fr language
                middleware.Add(new ConversationState<CurrentUserState>(new MemoryStorage()));
                Dictionary<string, Dictionary<string, string>> userCustomDictonaries = new Dictionary<string, Dictionary<string, string>>();
                Dictionary<string, string> frenctDictionary = new Dictionary<string, string>();
                frenctDictionary.Add("content", "excited");
                userCustomDictonaries.Add("fr", frenctDictionary);
                middleware.Add(new LocaleConverterMiddleware(TranslatorLocaleHelper.GetActiveLocale, TranslatorLocaleHelper.CheckUserChangedLanguageOrLocale, "en-us", LocaleConverter.Converter));
                middleware.Add(new TranslationMiddleware(new string[] { "en" }, "<your translator key here>", patterns, userCustomDictonaries, TranslatorLocaleHelper.GetActiveLanguage, TranslatorLocaleHelper.CheckUserChangedLanguageOrLocale, true)); 
                middleware.Add(new QnAMakerMiddleware(qnaEndpoint));

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