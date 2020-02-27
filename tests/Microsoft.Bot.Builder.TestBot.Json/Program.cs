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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Microsoft.Bot.Builder.TestBot.Json
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static void Help()
        {
            Trace.TraceInformation("--root <PATH>: Absolute path to the root directory for declarative resources all *.main.dialog be options.  Default current directory");
            Trace.TraceInformation("--region <REGION>: LUIS endpoint region.  Default westus");
            Trace.TraceInformation("--environment <ENVIRONMENT>: LUIS environment settings to use.  Default is user alias.");
            Trace.TraceInformation("--help: This help.");
            System.Environment.Exit(-1);
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((hostingContext, builder) =>
            {
                builder.UseLuisSettings();
            })
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });
    }
}
