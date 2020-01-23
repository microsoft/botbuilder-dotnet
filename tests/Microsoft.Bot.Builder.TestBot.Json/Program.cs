// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection.Emit;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Bot.Builder.TestBot.Json
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static void Help()
        {
            Trace.TraceInformation("--root <PATH>: Absolute path to the root directory for declarative resources all *.main.dialog be options.  Default current directory");
            Trace.TraceInformation("--region <REGION>: LUIS endpoint region.  Default westus");
            Trace.TraceInformation("--environment <ENVIRONMENT>: LUIS environment settins to use.  Default 'devlopment' or user alias.");
            Trace.TraceInformation("--help: This help.");
            System.Environment.Exit(-1);
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
                var luisRegion = Environment.GetEnvironmentVariable("LUIS_AUTHORING_REGION") ?? "westus";
                var environment = "development";
                var botRoot = ".";
                for (var i = 0; i < args.Length; ++i)
                {
                    var arg = args[i];
                    switch (arg)
                    {
                        case "--region":
                            {
                                if (++i < args.Length)
                                {
                                    luisRegion = args[i];
                                }
                            }

                            break;
                        case "--root":
                            {
                                if (++i < args.Length)
                                {
                                    botRoot = args[i];
                                }
                            }

                            break;
                        case "--environment":
                            {
                                if (++i < args.Length)
                                {
                                    environment = args[i];
                                }
                            }

                            break;

                        default: Help(); break;
                    }
                }

                var settings = new Dictionary<string, string>();
                settings["luis:endpoint"] = $"https://{luisRegion}.api.cognitive.microsoft.com";
                settings["BotRoot"] = botRoot;
                config.AddInMemoryCollection(settings);

                config.AddUserSecrets<Startup>();

                // Add general and then user specific luis.settings files to config
                var di = new DirectoryInfo(botRoot);
                var generalPattern = $"{environment}.{luisRegion}.json";
                foreach (var file in di.GetFiles($"luis.settings.{generalPattern}", SearchOption.AllDirectories))
                {
                    config.AddJsonFile(file.FullName, optional: false, reloadOnChange: true);
                }

                var userPattern = $"{Environment.UserName}.{luisRegion}.json";
                foreach (var file in di.GetFiles($"luis.settings.{userPattern}", SearchOption.AllDirectories))
                {
                    config.AddJsonFile(file.FullName, optional: false, reloadOnChange: true);
                }
            })
            .UseStartup<Startup>()
            .Build();
    }
}
