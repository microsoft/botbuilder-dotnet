// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.IO;
using Microsoft.Bot.Builder.AI.Luis.Testing;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Testing;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Profiling
{
    // This program is necessary because Visual Studio cannot currently profile tests.
    // With this program you can point to declarative tests and execute them while profiling.
    public class Program
    {
        public static void Help()
        {
            Console.Error.WriteLine("[-n <iterations>] [-secret <id>] [-luis <luisDir>] testscripts...");
            Console.Error.WriteLine("-secret This is your dotnet secret id for luis keys. [default = 'profile']");
            Console.Error.WriteLine("-luis This is the directory where luis settings are.");
            Console.Error.WriteLine("-n Is number of iterations to do.");
            Console.Error.WriteLine("The - parameters are applied to each testscript in order so you can change them per testscript.");
            Console.Error.WriteLine("Example: -secret GeneratorId -luis sandwich/luis generator_sandwich.test.dialog");
            System.Environment.Exit(-1);
        }

        public static void Main(string[] args)
        {
            var secret = "profile";
            string luis = null;
            var iterations = 1;
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
                            throw new System.ArgumentException("Missing -secret value");
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
                            throw new System.ArgumentException("Missing -luis value");
                        }
                    }
                    else if (arg == "-n")
                    {
                        if (++i < args.Length)
                        {
                            iterations = int.Parse(args[i]);
                        }
                        else
                        {
                            throw new System.ArgumentException("Missing -n value");
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
                        .UseMockLuisSettings(luis ?? dir, secret)
                        .Build();

                    var explorer = new ResourceExplorer()
                        .AddFolder(dir, monitorChanges: false)
                        .RegisterType(LuisAdaptiveRecognizer.Kind, typeof(MockLuisRecognizer), new MockLuisLoader()); 
                    HostContext.Current.Set(config);
                    var script = explorer.LoadType<TestScript>(name);
                    var timer = new System.Diagnostics.Stopwatch();
                    Console.WriteLine($"Executing {arg} for {iterations} iterations");
                    timer.Start();
                    var adapter = script.DefaultTestAdapter(testName: name, resourceExplorer: explorer);
                    timer.Stop();
                    var loading = timer.ElapsedMilliseconds;
                    Console.WriteLine($" loading took {loading} ms");

                    var iterationTime = 0L;
                    var firstTime = 0l;
                    for (var iter = 0; iter < iterations; ++iter)
                    {
                        timer.Restart();
                        script.ExecuteAsync(explorer, adapter: adapter).Wait();
                        timer.Stop();
                        if (firstTime > 0)
                        {
                            iterationTime += timer.ElapsedMilliseconds;
                        }
                        else
                        {
                            firstTime = timer.ElapsedMilliseconds;
                        }

                        Console.WriteLine($"  {iter}: {timer.ElapsedMilliseconds} ms");
                    }

                    Console.Write($" Total time={loading + firstTime + iterationTime} ms");
                    if (iterations > 1)
                    {
                        Console.WriteLine($", per iteration after 1st={iterationTime / ((float)iterations - 1)} ms");
                    }
                }
            }
        }
    }
}
