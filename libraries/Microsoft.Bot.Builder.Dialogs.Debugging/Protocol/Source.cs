// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.Dialogs.Debugging.Protocol
{
    public sealed class Source
    {
        public Source(string path)
        {
            Name = System.IO.Path.GetFileName(path);
            Path = path;
        }

        public string Name { get; set; }

        public string Path { get; set; }
    }
}
