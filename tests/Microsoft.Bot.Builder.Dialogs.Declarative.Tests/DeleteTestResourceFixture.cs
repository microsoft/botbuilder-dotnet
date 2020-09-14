// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.IO;

namespace Microsoft.Bot.Builder.Dialogs.Declarative.Tests
{
    public class DeleteTestResourceFixture : IDisposable
    {
        public DeleteTestResourceFixture()
        {
            var path = Path.GetFullPath(PathUtils.NormalizePath(Path.Combine(Environment.CurrentDirectory, @"..\..")));
            foreach (var file in Directory.EnumerateFiles(path, "*.dialog", SearchOption.AllDirectories))
            {
                File.Delete(file);
            }
        }

        public void Dispose()
        {
        }
    }
}
