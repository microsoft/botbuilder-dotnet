using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Bot.Builder.Integration.ApplicationInsights.Core.Tests
{
    internal class StartupNoParameters
    {
        public StartupNoParameters(IHostingEnvironment env)
        {
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddBotApplicationInsights();

            // Adding IConfiguration in sample test server.  Otherwise this appears to be
            // registered.
            services.AddSingleton<IConfiguration>();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseBotApplicationInsights();
        }
    }
}
