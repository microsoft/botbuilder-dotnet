using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.BotFramework;
using Microsoft.Bot.Builder.Middleware;
using Microsoft.Bot.Builder.Storage;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.Bot.Samples.EchoBot_ASPNetCore_DI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Register required services.
            services.AddSingleton<IConfiguration>(this.Configuration);
            services.AddSingleton<ICredentialProvider, ConfigurationCredentialProvider>();
            services.AddSingleton<IStorage, MemoryStorage>();
            services.AddTransient<HttpClient, HttpClient>();
            services.AddSingleton<ConversationStateManagerMiddleware, ConversationStateManagerMiddleware>();
            services.AddSingleton<Builder.Middleware.IMiddleware>(this.GetMiddlewareSet);
            services.AddSingleton<BotAdapter, BotFrameworkAdapter>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<BotAdapterHelper, BotAdapterHelper>();

            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }

        /// <summary>
        /// Gets the middleware set. Collection of all middlewares to use with Adapter.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <returns>Middleware(s) to be used.</returns>
        private Builder.Middleware.IMiddleware GetMiddlewareSet(IServiceProvider serviceProvider)
        {
            MiddlewareSet middlewareSet = new MiddlewareSet();
            var stateMangerMiddlware = serviceProvider.GetService<ConversationStateManagerMiddleware>();

            if (stateMangerMiddlware != null)
            {
                middlewareSet.Use(stateMangerMiddlware);
            }

            return middlewareSet;
        }
    }
}
