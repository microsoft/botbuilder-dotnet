// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Immutable;
using Microsoft.Bot.Builder.Dialogs.Memory.Scopes;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Bot.Builder.Integration.Runtime
{
    internal class ConfigurationSettingsMemoryScope : SettingsMemoryScope
    {
        public static new ImmutableDictionary<string, object> LoadSettings(IConfiguration configuration)
        {
            return SettingsMemoryScope.LoadSettings(configuration);
        }
    }
}
