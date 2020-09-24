// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.IO;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;

namespace Microsoft.Bot.Builder.Dialogs.Loader.Tests
{
    public class ResourceExplorerFixture : IDisposable
    {
        public ResourceExplorerFixture()
        {
            ResourceExplorer = new ResourceExplorer();
            NoCycleResourceExplorer = new ResourceExplorer(new ResourceExplorerOptions { AllowCycles = false });
            Initialize();
        }

        public ResourceExplorer ResourceExplorer { get; set; }

        public ResourceExplorer NoCycleResourceExplorer { get; set; }

        public ResourceExplorerFixture Initialize()
        {
            var projPath = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, PathUtils.NormalizePath($@"..\..\..\..\..\tests\Microsoft.Bot.Builder.TestBot.Json\Microsoft.Bot.Builder.TestBot.Json.csproj")));

            ResourceExplorer = new ResourceExplorer()
                .LoadProject(projPath, monitorChanges: false);

            NoCycleResourceExplorer = new ResourceExplorer(options: new ResourceExplorerOptions() { AllowCycles = false })
                .LoadProject(projPath, monitorChanges: false);

            return this;
        }

        public void Dispose()
        {
            ResourceExplorer.Dispose();
            NoCycleResourceExplorer.Dispose();
        }
    }
}
