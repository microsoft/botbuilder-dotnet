using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Storage;
using Micosoft.Bot.Samples.InjectionBasedBotExample;
using System.Threading;

namespace InjectionBasedBotExample
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
            services.AddMvc();
            CreateBot(services);
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

        public void CreateBot(IServiceCollection services)
        {
            services.UseBotConnector();
            services.AddSingleton<IStorage>(new MemoryStorage());            
            services.AddScoped<Bot>(serviceProvider =>
              {                  
                  Bot b = new Bot(new BotFrameworkConnector("", ""))
                    .Use((IMiddleware)serviceProvider.GetService<IStorage>())
                    .Use(new BotStateManager())
                    .Use(new EchoMiddleware());

                  return b;
              });
        }
    }
}
