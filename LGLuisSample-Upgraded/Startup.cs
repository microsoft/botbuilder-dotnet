using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Builder.BotFramework;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Ai.LUIS;
using System.Linq;

namespace LGLuisSample_Upgraded
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
                //var luisModel = new LuisModel(luisModelId, luisSubscriptionKey, luisUri);
                var luisApplication = new LuisApplication(luisModelId, luisSubscriptionKey, "westus");
                var luisRecognizer = new LuisRecognizer(luisApplication);

                IStorage dataStore = new MemoryStorage();
                //Microsoft.Bot.Builder.Core.Extensions.IStorage dataStore = new Microsoft.Bot.Builder.Core.Extensions.MemoryStorage();
                //options.Middleware.Add(new ConversationState(dataStore));
                //options.Middleware.Add(new UserState(dataStore));
                var converstationState = new ConversationState(dataStore);
                options.State.Add(converstationState);

                var userState = new UserState(dataStore);
                options.State.Add(userState);

                var stateSet = new BotStateSet(options.State.ToArray());
                options.Middleware.Add(stateSet);

                //options.Middleware.Add(new LuisRecognizerMiddleware(luisModel, luisOptions: luisOptions));
                //options.Middleware.Add(new UserState<UserState>(dataStore));
                options.Middleware.Add(new LuisRecognizerMiddleware(luisRecognizer));
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
