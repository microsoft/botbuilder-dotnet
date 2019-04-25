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
                var luisAuthoringRegion = Environment.GetEnvironmentVariable("LUIS_AUTHORING_REGION") ?? "westus";
                config
                    .AddJsonFile($"luis.settings.{env.EnvironmentName}.{luisAuthoringRegion}.json", optional: true, reloadOnChange: true)
                    .AddJsonFile($"luis.settings.{Environment.UserName}.{luisAuthoringRegion}.json", optional: true, reloadOnChange: true);
            }).UseStartup<Startup>()
            .Build();
    }
}
