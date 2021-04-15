// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Extensions.Configuration;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Runtime
{
    internal class ConfigurationLanguagePolicy : LanguagePolicy
    {
        private const string DefaultLocale = "en-US";

        public ConfigurationLanguagePolicy(IConfiguration configuration)
            : base(configuration.GetSection(ConfigurationConstants.DefaultLocaleKey).Value ?? DefaultLocale)
        {
        }
    }
}
