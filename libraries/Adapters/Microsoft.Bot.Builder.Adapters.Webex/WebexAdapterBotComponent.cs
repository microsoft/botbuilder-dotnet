// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Bot.Builder.Adapters.Webex
{
    /// <summary>
    /// <see cref="BotComponent"/> definition for <see cref="WebexAdapter"/>.
    /// </summary>
    public class WebexAdapterBotComponent : BotComponent
    {
        /// <inheritdoc/>
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            if (WebexAdapter.HasConfiguration(configuration))
            {
                services.AddSingleton<WebexAdapter>();
            }
        }
    }
}
