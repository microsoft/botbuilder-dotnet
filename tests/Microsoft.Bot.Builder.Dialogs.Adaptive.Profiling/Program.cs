// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.AI.QnA;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Testing;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Builder.Dialogs.Declarative.Types;
using Microsoft.Bot.Builder.MockLuis;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Profiling
{
    // This program is necessary because Visual Studio cannot currently profile tests.
    // With this program you can point to declarative tests and execute them while profiling.
    public class Program
    {
        public static void Help()
        {
            Console.Error.WriteLine("[-secret <id=profile>] [-luis <luisDir>] testscripts...");
            Console.Error.WriteLine("-secret This is your secret id for luis keys.");
            Console.Error.WriteLine("-luis This is the directory where luis settings arg.");
            System.Environment.Exit(-1);
        }

        public static void Main(string[] args)
        {
            var secret = "profile";
            string luis = null;
            if (args.Length == 0)
            {
                Help();
            }

            for (var i = 0; i < args.Length; ++i)
            {
                var arg = args[i];
                if (arg.StartsWith("-"))
                {
                    if (arg == "-secret")
                    {
                        if (++i < args.Length)
                        {
                            secret = args[i];
                        }
                        else
                        {
                            throw new System.ArgumentException("Missing --secret value");
                        }
                    }
                    else if (arg == "-luis")
                    {
                        if (++i < args.Length)
                        {
                            luis = args[i];
                        }
                        else
                        {
                            throw new System.ArgumentException("Missing --luis value");
                        }
                    }
                    else
                    {
                        Console.Error.WriteLine($"Unknown arg {arg}");
                        Help();
                    }
                }
                else
                {
                    var cd = Directory.GetCurrentDirectory();
                    var dir = Path.GetDirectoryName(arg);
                    var name = Path.GetFileName(arg);
                    var config = new ConfigurationBuilder()
                        .AddInMemoryCollection()
                        .UseLuisSettings(luis ?? dir, secret)
                        .Build();
                    var explorer = new ResourceExplorer().AddFolder(dir);
                    DeclarativeTypeLoader.Reset();
                    TypeFactory.Configuration = config;
                    DeclarativeTypeLoader.AddComponent(new DialogComponentRegistration());
                    DeclarativeTypeLoader.AddComponent(new AdaptiveComponentRegistration());
                    DeclarativeTypeLoader.AddComponent(new LanguageGenerationComponentRegistration());
                    DeclarativeTypeLoader.AddComponent(new QnAMakerComponentRegistration());
                    DeclarativeTypeLoader.AddComponent(new MockLuisComponentRegistration());
                    var script = explorer.LoadType<TestScript>(name);
                    var timer = new System.Diagnostics.Stopwatch();
                    Console.Write($"Executing {arg}");
                    timer.Start();
                    script.ExecuteAsync(testName: name, configuration: config, resourceExplorer: explorer).Wait();
                    timer.Stop();
                    Console.WriteLine($" took {timer.ElapsedMilliseconds} ms");
                }
            }
        }
    }
}
