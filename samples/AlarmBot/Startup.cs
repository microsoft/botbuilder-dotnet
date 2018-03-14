// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using AlarmBot.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Text.RegularExpressions;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder.Core.Extensions;

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
            services.AddBot<AlarmBot>(options =>
            {
                options.CredentialProvider = new SimpleCredentialProvider(Configuration.GetSection(MicrosoftAppCredentials.MicrosoftAppIdKey)?.Value, Configuration.GetSection(MicrosoftAppCredentials.MicrosoftAppPasswordKey)?.Value);
                var middleware = options.Middleware;

                middleware.Add(new UserState<UserData>(new MemoryStorage()));
                middleware.Add(new ConversationState<ConversationData>(new MemoryStorage()));
                middleware.Add(new BatchOutputMiddleware());
                middleware.Add(new RegExpRecognizerMiddleware()
                                .AddIntent("showAlarms", new Regex("show alarm(?:s)*(.*)", RegexOptions.IgnoreCase))
                                .AddIntent("addAlarm", new Regex("add(?: an)* alarm(.*)", RegexOptions.IgnoreCase))
                                .AddIntent("deleteAlarm", new Regex("delete(?: an)* alarm(.*)", RegexOptions.IgnoreCase))
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
            app.UseBotFramework();
        }
    }
}
