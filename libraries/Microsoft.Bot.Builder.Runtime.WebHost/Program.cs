// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder.Runtime;
using Microsoft.Bot.Builder.Runtime.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Microsoft.Bot.Builder.Runtime.WebHost
{
    public class Program
    {
        private const string AppSettingsRelativePath = @"settings/appsettings.json";

        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((hostingContext, builder) =>
            {
                IHostEnvironment env = hostingContext.HostingEnvironment;

                // Use Composer bot path adapter
                builder.AddBotCoreConfiguration(
                    applicationRoot: Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                    isDevelopment: env.IsDevelopment());

                IConfiguration configuration = builder.Build();

                string botRootPath = configuration.GetValue<string>(ConfigurationConstants.BotKey);
                string configFilePath = Path.GetFullPath(Path.Combine(botRootPath, AppSettingsRelativePath));

                builder.AddJsonFile(configFilePath, optional: true, reloadOnChange: true);

                // Use Composer luis and qna settings extensions
                builder.AddComposerConfiguration();

                builder.AddEnvironmentVariables()
                    .AddCommandLine(args);
            })
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });
    }
}
