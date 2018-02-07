// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Micosoft.Bot.Samples.InjectionBasedBotExample;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.BotFramework;
using Microsoft.Bot.Builder.Storage;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace InjectionBasedBotExample
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

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(_ => Configuration);            
            services.AddMvc();

            CreateBot(services);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseDefaultFiles();
            app.UseStaticFiles();            
            app.UseMvc();
        }

        public void CreateBot(IServiceCollection services)
        {            
            string appId = Configuration.GetSection(MicrosoftAppCredentials.MicrosoftAppIdKey)?.Value ?? string.Empty;
            string appKey = Configuration.GetSection(MicrosoftAppCredentials.MicrosoftAppPasswordKey).Value ?? string.Empty;

            // Memory state store as a sigleton, so data is recalled across messages            
            services.AddSingleton<IStorage>(new MemoryStorage());

            // Bot is created on each request
            services.AddScoped<Bot>(serviceProvider =>
              {
                  Bot b = new Bot(new BotFrameworkAdapter(appId, appKey))                    
                    .Use(new BotStateManager(serviceProvider.GetService<IStorage>()))
                    .Use(new EchoMiddleware());

                  return b;
              });

            /*** Create the entire Bot as a Singleton **/
            //services.AddSingleton<Bot>(serviceProvider =>
            //{
            //    Bot b = new Bot(new BotFrameworkAdapter(appId, appKey))
            //      .Use(new BotStateManager(new MemoryStorage()))
            //      .Use(new EchoMiddleware());
            //    return b;
            //});
        }
    }
}
