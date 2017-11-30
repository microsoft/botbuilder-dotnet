using Microsoft.Bot.Connector;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Storage;
using Micosoft.Bot.Samples.InjectionBasedBotExample;

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
            // TODO THIS IS DIFFERENT WITH NEW CONNECTOR LIB
            //services.UseBotConnector();

            /*** Create just the Memory state store as a sigleton,
             *      and keep the Bot created on each request **/
            services.AddSingleton<IStorage>(new MemoryStorage());
            services.AddScoped<Bot>(serviceProvider =>
              {
                  Bot b = new Bot(new BotFrameworkAdapter("", ""))
                    .Use((IMiddleware)serviceProvider.GetService<IStorage>())
                    .Use(new BotStateManager())
                    .Use(new EchoMiddleware());

                  return b;
              });

            /*** Create the entire Bot as a Singleton **/
            //services.AddSingleton<Bot>(serviceProvider =>
            //{
            //    Bot b = new Bot(new BotFrameworkConnector("", ""))
            //      .Use(new MemoryStorage())
            //      .Use(new BotStateManager())
            //      .Use(new EchoMiddleware());

            //    return b;
            //});
        }
    }
}
