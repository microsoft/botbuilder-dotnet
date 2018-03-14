using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder.BotFramework;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Bot.Builder.Ai;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.LUIS;
using System;
using Microsoft.Cognitive.LUIS;

namespace Microsoft.Bot.Samples.LUIS
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(_ => Configuration);
            services.AddBot<LUISBot>(options =>
            {
                options.CredentialProvider = new ConfigurationCredentialProvider(Configuration);
                var middleware = options.Middleware;

                // Setup LUIS Middleware
                var luisRecognizerOptions = new LuisRecognizerOptions { Verbose = false };
                var luisModel = new LuisModel(
                    "<APPLICATION ID>", 
                    "<SUBSCRIPTION KEY>", 
                    new Uri("https://westus.api.cognitive.microsoft.com/luis/v2.0/apps/"));
                middleware.Add(new LuisRecognizerMiddleware(luisModel, luisRecognizerOptions));

            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseBotFramework();
        }
    }
}
