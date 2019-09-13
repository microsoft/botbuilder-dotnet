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
                    .AddUserSecrets<Startup>()
                    .AddJsonFile($"luis.settings.{env.EnvironmentName}.{luisAuthoringRegion}.json", optional: true, reloadOnChange: false)
                    .AddJsonFile($"luis.settings.{Environment.UserName}.{luisAuthoringRegion}.json", optional: true, reloadOnChange: false);
            }).UseStartup<Startup>()
            .Build();
    }
}
