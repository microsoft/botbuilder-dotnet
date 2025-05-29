﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Configuration;
using Microsoft.Bot.Connector.Authentication;

namespace Microsoft.Bot.Builder.BotFramework
{
    /// <summary>
    /// Loads credentials from the <see cref="ConfigurationManager.AppSettings"/>.
    /// </summary>
    /// <remarks>
    /// This will populate the <see cref="SimpleCredentialProvider.AppId"/> from an app setting entry with the key of <see cref="MicrosoftAppCredentials.MicrosoftAppIdKey"/>
    /// and the <see cref="SimpleCredentialProvider.Password"/> from an app setting with the key of <see cref="MicrosoftAppCredentials.MicrosoftAppPasswordKey"/>.
    ///
    /// NOTE: if the keys are not present, a <c>null</c> value will be used.
    /// </remarks>
    [Obsolete("Use `ConfigurationBotFrameworkAuthentication` instead to configure credentials.", false)]
    public sealed class ConfigurationCredentialProvider : SimpleCredentialProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationCredentialProvider"/> class.
        /// </summary>
        public ConfigurationCredentialProvider()
        {
            this.AppId = ConfigurationManager.AppSettings[MicrosoftAppCredentials.MicrosoftAppIdKey];
            this.Password = ConfigurationManager.AppSettings[MicrosoftAppCredentials.MicrosoftAppPasswordKey];
        }
    }
}
