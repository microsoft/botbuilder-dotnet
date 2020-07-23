<<<<<<< HEAD
﻿using Microsoft.AspNetCore.Hosting;
=======
﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Hosting;
>>>>>>> f127fca9b2eef1fe51f52bbfb2fbbab8a10fc0e8
using Microsoft.Extensions.Hosting;

namespace Microsoft.BotBuilderSamples.AdaptiveRootBot
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
