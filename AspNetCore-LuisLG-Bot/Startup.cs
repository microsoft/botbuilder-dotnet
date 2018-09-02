using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Ai.LUIS;
using Microsoft.Bot.Builder.BotFramework;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Cognitive.LUIS;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore_LuisLG_Bot
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
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddBot<LuisLanguageGenerationBot>(options =>
            {
                options.CredentialProvider = new ConfigurationCredentialProvider(Configuration);

                string luisModelId = Keys.LuisModelId;
                string luisSubscriptionKey = Keys.LuisSubscriptionKey;
                Uri luisUri = new Uri("https://westus.api.cognitive.microsoft.com/luis/v2.0/apps/");
                var luisModel = new LuisModel(luisModelId, luisSubscriptionKey, luisUri);

                // If you want to get all intents scorings, add verbose in luisOptions
                var luisOptions = new LuisRequest { Verbose = true };
                // The Memory Storage used here is for local bot debugging only. When the bot
                // is restarted, anything stored in memory will be gone. 



                Microsoft.Bot.Builder.Core.Extensions.IStorage dataStore = new Microsoft.Bot.Builder.Core.Extensions.MemoryStorage();
                options.Middleware.Add(new ConversationState<Dictionary<string, object>>(dataStore));
                options.Middleware.Add(new UserState<UserState>(dataStore));
                options.Middleware.Add(new LuisRecognizerMiddleware(luisModel, luisOptions: luisOptions));
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
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
