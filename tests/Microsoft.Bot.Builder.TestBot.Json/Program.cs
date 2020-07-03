// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection.Emit;
using Microsoft.AspNetCore;
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
            Console.WriteLine("To use LUIS you should do 'dotnet user-secrets --id TestBot set luis:endpointKey=<yourKey>'");
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((hostingContext, builder) =>
            {
                var configuration = builder.Build();
                var botRoot = configuration.GetValue<string>("root") ?? ".";
                var region = configuration.GetValue<string>("region") ?? "westus";
                var environment = configuration.GetValue<string>("environment") ?? Environment.UserName;

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
