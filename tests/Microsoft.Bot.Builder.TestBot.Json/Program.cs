// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using System.IO;
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
            Trace.TraceInformation("--region <REGION>: LUIS Authoring region.  Default westus");
            Trace.TraceInformation("--help: This help.");
            Trace.TraceInformation("--root: Root directory for declartive resources.");
            System.Environment.Exit(-1);
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
                var env = hostingContext.HostingEnvironment;
                var luisAuthoringRegion = Environment.GetEnvironmentVariable("LUIS_AUTHORING_REGION") ?? "westus";
                for (var i = 0; i < args.Length; ++i)
                {
                    var arg = args[i];
                    switch (arg)
                    {
                        case "--region":
                            {
                                if (++i < args.Length)
                                {
                                    luisAuthoringRegion = args[i];
                                }
                            }

                            break;
                        case "--root":
                            {
                                if (++i < args.Length)
                                {
                                    env.ContentRootPath = args[i];
                                }
                            }

                            break;
                        default: Help(); break;
                    }
                }

                config.AddUserSecrets<Startup>();

                // Add general and then user specific luis.settings files to config
                var di = new DirectoryInfo(env.ContentRootPath);
                var generalPattern = $"{env.EnvironmentName}.{luisAuthoringRegion}.json";
                foreach (var file in di.GetFiles($"luis.settings.{generalPattern}", SearchOption.AllDirectories))
                {
                    config.AddJsonFile(file.FullName, optional: false, reloadOnChange: true);
                }

                var userPattern = $"{Environment.UserName}.{luisAuthoringRegion}.json";
                foreach (var file in di.GetFiles($"luis.settings.{userPattern}", SearchOption.AllDirectories))
                {
                    config.AddJsonFile(file.FullName, optional: false, reloadOnChange: true);
                }
            })
            .UseStartup<Startup>()
            .Build();
    }
}
