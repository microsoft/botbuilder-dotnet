﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Bot.Builder.Adapters.Slack
{
    /// <summary>
    /// <see cref="BotComponent"/> definition for <see cref="SlackAdapter"/>.
    /// </summary>
    [Obsolete("The Bot Framework Adapters will be deprecated in the next version of the Bot Framework SDK and moved to https://github.com/BotBuilderCommunity/botbuilder-community-dotnet. Please refer to their new location for all future work.")]
    public class SlackAdapterBotComponent : BotComponent
    {
        /// <inheritdoc/>
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            if (SlackAdapter.HasConfiguration(configuration))
            {
                // Components require the component configuration which is the subsection
                // assigned to the component. When the botbuilder-dotnet issue #5583 gets resolved, this could
                // change to the no-parameter overload.
                services.AddSingleton<IBotFrameworkHttpAdapter, SlackAdapter>(sp => new SlackAdapter(configuration));
            }
        }
    }
}
