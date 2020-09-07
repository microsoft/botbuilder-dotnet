// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Tests
{
    public class ConfigurationFixture : IDisposable
    {
        public ConfigurationFixture()
        {
            Configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                .Build();
        }

        public IConfiguration Configuration { get; private set; }

        public void Dispose()
        {
            Configuration = null;
        }
    }
}
