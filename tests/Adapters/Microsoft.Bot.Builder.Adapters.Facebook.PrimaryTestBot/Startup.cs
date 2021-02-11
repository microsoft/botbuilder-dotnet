// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder.Adapters.Facebook.PrimaryTestBot;
using Microsoft.Bot.Builder.Adapters.Facebook.PrimaryTestBot.Bots;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Bot.Builder.Adapters.Facebook.TestBot
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers().AddNewtonsoftJson();

            // Create the Bot Framework Facebook Adapter.
            services.AddSingleton<IBotFrameworkHttpAdapter, FacebookAdapterWithTag>();

            // Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
            services.AddTransient<IBot, PrimaryBot>();
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
                .UseRouting()
                .UseAuthorization()
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                });

            // app.UseHttpsRedirection();
        }
    }
}
