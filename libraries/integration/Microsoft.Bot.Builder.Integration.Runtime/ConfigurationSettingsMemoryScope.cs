// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Immutable;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Memory.Scopes;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Bot.Builder.Integration.Runtime
{
    internal class ConfigurationSettingsMemoryScope : SettingsMemoryScope
    {
        private readonly ImmutableDictionary<string, object> _settings;

        public ConfigurationSettingsMemoryScope(IConfiguration configuration)
        {
            _settings = LoadSettings(configuration);
        }

        public override object GetMemory(DialogContext dc)
        {
            if (dc == null)
            {
                throw new ArgumentNullException(nameof(dc));
            }

            if (!dc.Context.TurnState.TryGetValue(ScopePath.Settings, out var settings))
            {
                dc.Context.TurnState[ScopePath.Settings] = _settings;
            }

            return _settings;
        }
    }
}
