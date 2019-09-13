// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.Bot.Builder.Dialogs.Declarative.Plugins
{
    public class FileDependencyInfo
    {
        public string AssemblyPath { get; set; }

        public string SchemaUri { get; set; }

        public string ClassName { get; set; }

        public string CustomLoaderClassName { get; set; }
    }
}
