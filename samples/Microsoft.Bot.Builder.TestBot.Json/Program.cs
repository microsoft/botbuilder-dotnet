// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Bot.Builder.TestBot.Json
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
                var env = hostingContext.HostingEnvironment;
                config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                      .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);

                foreach (var region in new string[] {  "westus", "westeurope", "australiaeast"})
                {
                    config.AddJsonFile($"luis.settings.{env.EnvironmentName}.{region}.json", optional: true, reloadOnChange: true);
                }
                foreach (var region in new string[] { "westus", "westeurope", "australiaeast" })
                {
                    config.AddJsonFile($"luis.settings.{Environment.UserName}.{region}.json", optional: true, reloadOnChange: true);
                }
                config.AddEnvironmentVariables();

                config.AddCommandLine(args);
            }).UseStartup<Startup>()
            .Build();
    }
}
