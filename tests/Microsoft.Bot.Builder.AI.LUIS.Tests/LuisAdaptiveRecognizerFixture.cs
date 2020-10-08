// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.IO;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Builder.AI.Luis.Testing;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Tests
{
    public class LuisAdaptiveRecognizerFixture : IDisposable
    {
        private readonly string dynamicListsDirectory = PathUtils.NormalizePath(@"..\..\..\tests\LuisAdaptiveRecognizerTests");

        public LuisAdaptiveRecognizerFixture()
        {
            Configuration = new ConfigurationBuilder()
                .UseMockLuisSettings(dynamicListsDirectory, "TestBot")
                .Build();

            ResourceExplorer = new ResourceExplorer()
                .AddFolder(Path.Combine(TestUtils.GetProjectPath(), "tests", nameof(LuisAdaptiveRecognizerTests)), monitorChanges: false)
                .RegisterType(LuisAdaptiveRecognizer.Kind, typeof(MockLuisRecognizer), new MockLuisLoader(Configuration));
        }

        public ResourceExplorer ResourceExplorer { get; set; }

        public IConfiguration Configuration { get; set; }

        public void Dispose()
        {
            ResourceExplorer.Dispose();
            Configuration = null;
        }
    }
}
