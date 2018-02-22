// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Middleware;
using Microsoft.Bot.Builder.Storage;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Text.RegularExpressions;

namespace AlarmBot
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
            services.AddMvc();

            // register adapater
            services.AddSingleton<BotFrameworkAdapter>(serviceProvider =>
            {
                string applicationId = Configuration.GetSection(MicrosoftAppCredentials.MicrosoftAppIdKey)?.Value;
                string applicationPassword = Configuration.GetSection(MicrosoftAppCredentials.MicrosoftAppPasswordKey)?.Value;

                // create bot hooked up to the activity adapater
                return new BotFrameworkAdapter(applicationId, applicationPassword)
                    .Use(new UserStateManagerMiddleware(new MemoryStorage()))
                    .Use(new ConversationStateManagerMiddleware(new MemoryStorage()))
                    .Use(new RegExpRecognizerMiddleware()
                        .AddIntent("showAlarms", new Regex("show alarms(.*)", RegexOptions.IgnoreCase))
                        .AddIntent("addAlarm", new Regex("add alarm(.*)", RegexOptions.IgnoreCase))
                        .AddIntent("deleteAlarm", new Regex("delete alarm(.*)", RegexOptions.IgnoreCase))
                        .AddIntent("help", new Regex("help(.*)", RegexOptions.IgnoreCase))
                        .AddIntent("cancel", new Regex("cancel(.*)", RegexOptions.IgnoreCase))
                        .AddIntent("confirmYes", new Regex("(yes|yep|yessir|^y$)", RegexOptions.IgnoreCase))
                        .AddIntent("confirmNo", new Regex("(no|nope|^n$)", RegexOptions.IgnoreCase)));
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
            app.UseMvc();
        }
    }
}
