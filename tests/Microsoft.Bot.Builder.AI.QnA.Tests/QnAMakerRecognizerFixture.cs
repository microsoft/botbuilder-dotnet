// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Linq;

namespace Microsoft.Bot.Builder.AI.QnA.Tests
{
    public class QnAMakerRecognizerFixture : IDisposable
    {
        public QnAMakerRecognizerFixture()
        {
            var parent = Environment.CurrentDirectory;
            while (!string.IsNullOrEmpty(parent))
            {
                if (Directory.EnumerateFiles(parent, "*proj").Any())
                {
                    break;
                }

                parent = Path.GetDirectoryName(parent);
            }
        }

        public void Dispose()
        {
        }
    }
}
