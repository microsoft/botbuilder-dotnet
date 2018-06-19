// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.BotFramework;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Rest.TransientFaultHandling;

namespace Microsoft.Bot.Samples.Dialog.Prompts
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
            //services.AddBot<OAuthPromptBot>(options =>
            services.AddBot<WaterfallBot>((Action<BotFrameworkOptions>)(options =>
            //services.AddBot<WaterfallNestedBot>(options =>
            //services.AddBot<WaterfallAndPromptBot>(options =>
            //services.AddBot<DialogContainerBot>(options =>
            {
                options.CredentialProvider = new ConfigurationCredentialProvider(Configuration);
                options.EnableProactiveMessages = true;
                options.ConnectorClientRetryPolicy = new RetryPolicy(
                    new BotFrameworkHttpStatusCodeErrorDetectionStrategy(), 3, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(20), TimeSpan.FromSeconds(1));

                options.Middleware.Add(new ConversationState<Dictionary<string, object>>(new MemoryStorage()));
            }));
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
