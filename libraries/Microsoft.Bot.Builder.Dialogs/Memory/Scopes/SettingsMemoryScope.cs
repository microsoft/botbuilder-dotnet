// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Bot.Builder.Dialogs.Memory.Scopes
{
    /// <summary>
    /// SettingsMemoryscope maps "settings" -> IConfiguration.
    /// </summary>
    public class SettingsMemoryScope : MemoryScope
    {
        private Dictionary<string, object> emptySettings = new Dictionary<string, object>();

        public SettingsMemoryScope()
            : base(ScopePath.SETTINGS, isReadOnly: true)
        {
        }

        public override object GetMemory(DialogContext dc)
        {
            if (dc == null)
            {
                throw new ArgumentNullException(nameof(dc));
            }

            var namedScopes = GetScopesMemory(dc.Context);
            if (!namedScopes.TryGetValue(ScopePath.SETTINGS, out object settings))
            {
                var configuration = dc.Context.TurnState.Get<IConfiguration>();
                if (configuration != null)
                {
                    settings = Configuration.LoadSettings(configuration);
                    namedScopes[ScopePath.SETTINGS] = settings;
                }
            }

            return settings ?? emptySettings;
        }
    }
}
