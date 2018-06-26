﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder.BotFramework;
using Microsoft.Bot.Builder.Ai.QnA;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore_QnA_Bot
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
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
            services.AddMvc();
            
            services.AddBot<QnABot>(options =>
            {
                options.CredentialProvider = new ConfigurationCredentialProvider(Configuration);

                var qnaEndpoint = GetQnAMakerEndpoint(Configuration);
                var qnaMiddleware = new QnAMakerMiddleware(qnaEndpoint);

                options.Middleware.Add(qnaMiddleware);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();

            app.UseDefaultFiles()
                .UseStaticFiles()
                .UseBotFramework();
        }

        private QnAMakerEndpoint GetQnAMakerEndpoint(IConfiguration configuration)
        {
            var host = configuration.GetSection("QnAMaker-Host")?.Value;
            var knowledgeBaseId = configuration.GetSection("QnAMaker-KnowledgeBaseId")?.Value;
            var endpointKey = configuration.GetSection("QnAMaker-EndpointKey")?.Value;
            return new QnAMakerEndpoint
            {
                Host = host,
                KnowledgeBaseId = knowledgeBaseId,
                EndpointKey = endpointKey
            };
        }
    }
}
