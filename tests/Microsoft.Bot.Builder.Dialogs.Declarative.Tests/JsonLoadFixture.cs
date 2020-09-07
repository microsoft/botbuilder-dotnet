// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.IO;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;

namespace Microsoft.Bot.Builder.Dialogs.Loader.Tests
{
    public class JsonLoadFixture : IDisposable
    {
        public JsonLoadFixture()
        {
            var projPath = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, PathUtils.NormalizePath($@"..\..\..\..\..\tests\Microsoft.Bot.Builder.TestBot.Json\Microsoft.Bot.Builder.TestBot.Json.csproj")));
            
            ResourceExplorer = new ResourceExplorer()
                .LoadProject(projPath, monitorChanges: false);
        }

        public ResourceExplorer ResourceExplorer { get; set; }

        public void Dispose()
        {
            ResourceExplorer.Dispose();
        }
    }
}
