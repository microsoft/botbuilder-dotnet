using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder.BotFramework;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder.TraceExtensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace $safeprojectname$
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
            services.AddBot<EchoBot>(options =>
            {
                options.CredentialProvider = new ConfigurationCredentialProvider(Configuration);

                // The CatchExceptionMiddleware provides a top-level exception handler for your bot. 
                // Any exceptions thrown by other Middleware, or by your OnTurn method, will be 
                // caught here. To facilitate debugging, the exception is sent out, via Trace, 
                // to the emulator. Trace activities are NOT displayed to users, so in addition
                // an "Oops" message is sent.
                options.Middleware.Add(new CatchExceptionMiddleware<Exception>(async (context, exception) =>
                {
                    await context.TraceActivity("EchoBot Exception", exception);
                    await context.SendActivity("Sorry, it looks like something went wrong!");
                }));

                // The Memory Storage used here is for local bot debugging only. When the bot
                // is restarted, anything stored in memory will be gone. 
                IStorage dataStore = new MemoryStorage();

                // The File data store, shown here, is suitable for bots that run on 
                // a single machine and need durable state across application restarts.                 
                // IStorage dataStore = new FileStorage(System.IO.Path.GetTempPath());

                // For production bots use the Azure Table Store, Azure Blob, or 
                // Azure CosmosDB storage provides, as seen below. To include any of 
                // the Azure based storage providers, add the Microsoft.Bot.Builder.Azure 
                // Nuget package to your solution. That package is found at:
                //      https://www.nuget.org/packages/Microsoft.Bot.Builder.Azure/

                // IStorage dataStore = new Microsoft.Bot.Builder.Azure.AzureTableStorage("AzureTablesConnectionString", "TableName");
                // IStorage dataStore = new Microsoft.Bot.Builder.Azure.AzureBlobStorage("AzureBlobConnectionString", "containerName");

                options.Middleware.Add(new ConversationState<EchoState>(dataStore));
            });
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
                .UseBotFramework();
        }
    }
}
