using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder.BotFramework;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder.Ai.LUIS;
using Microsoft.Cognitive.LUIS;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.Bot.Samples.Ai.Luis.Dispatch
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
            services.AddBot<LuisDispatchBot>(options =>
            {
                options.CredentialProvider = new ConfigurationCredentialProvider(Configuration);

                string luisModelId = "<Your LUIS app ID from cDispatch here>";
                string luisSubscriptionKey = "<Your LUIS Subscription Key here>";
                Uri luisUri = new Uri("https://westus.api.cognitive.microsoft.com/luis/v2.0/apps/");

                var luisModel = new LuisModel(luisModelId, luisSubscriptionKey, luisUri);

                // If you want to get all intents scorings, add verbose in luisOptions
                var luisOptions = new LuisRequest { Verbose = true };

                var middleware = options.Middleware;
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
