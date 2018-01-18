using Microsoft.Bot.Connector;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Bot.Builder.Storage;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Adapters;
using Micosoft.Bot.Samples.InjectionBasedBotExample;

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
            var credentialProvider = new StaticCredentialProvider(
                Configuration.GetSection(MicrosoftAppCredentials.MicrosoftAppIdKey)?.Value,
                Configuration.GetSection(MicrosoftAppCredentials.MicrosoftAppPasswordKey)?.Value);

            services.AddAuthentication(
                    // This can be removed after https://github.com/aspnet/IISIntegration/issues/371
                    options =>
                    {
                        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                    }
                )
                .AddBotAuthentication(credentialProvider);

            services.AddSingleton(typeof(ICredentialProvider), credentialProvider);
            services.AddMvc(options =>
            {
                options.Filters.Add(typeof(TrustServiceUrlAttribute));
            });

            CreateBot(services);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseStaticFiles();
            app.UseAuthentication();
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
                    .Use((Microsoft.Bot.Builder.Middleware.IMiddleware)serviceProvider.GetService<IStorage>())
                    .Use(new BotStateManager())
                    .Use(new EchoMiddleware());

                  return b;
              });

            /*** Create the entire Bot as a Singleton **/
            //services.AddSingleton<Bot>(serviceProvider =>
            //{
            //    Bot b = new Bot(new BotFrameworkConnector(appId, appKey))
            //      .Use(new MemoryStorage())
            //      .Use(new BotStateManager())
            //      .Use(new EchoMiddleware());

            //    return b;
            //});
        }
    }
}
