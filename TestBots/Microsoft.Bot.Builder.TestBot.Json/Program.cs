﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Builder.AI.QnA;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Microsoft.Bot.Builder.TestBot.Json
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Help();
            CreateHostBuilder(args).Build().Run();
        }

        public static void Help()
        {
            Console.WriteLine("--root <PATH>: Absolute path to the root directory for declarative resources all *.main.dialog be options.  Default current directory");
            Console.WriteLine("--region <REGION>: LUIS endpoint region.  Default westus");
            Console.WriteLine("--environment <ENVIRONMENT>: LUIS environment settings to use.  Default is user alias.");
            Console.WriteLine("--dialog <DIALOG>: Name of root dialog to run.  By default all *.main.dialog will be choices.");
            Console.WriteLine("To use LUIS you should do 'dotnet user-secrets set --id TestBot luis:endpointKey <yourKey>'");
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((hostingContext, builder) =>
            {
                var configuration = builder.Build();
                var botRoot = configuration.GetValue<string>("root") ?? ".";
                var region = configuration.GetValue<string>("region") ?? "westus";
                var environment = configuration.GetValue<string>("environment") ?? Environment.UserName;
                var dialog = configuration.GetValue<string>("dialog");

                builder.UseLuisSettings();
                builder.UseQnAMakerSettings(botRoot, region, environment);
                builder.AddUserSecrets("TestBot");
            })
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });
    }
}
