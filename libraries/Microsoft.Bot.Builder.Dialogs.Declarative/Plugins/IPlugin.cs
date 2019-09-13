// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Declarative.Loaders;

namespace Microsoft.Bot.Builder.Dialogs.Declarative.Plugins
{
    public interface IPlugin
    {
        string SchemaUri { get; }

        Type Type { get; }

        ICustomDeserializer Loader { get; }

        Task Load();
    }
}
