// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Bot.Builder.Adapters.Twilio
{
    /// <summary>
    /// <see cref="BotComponent"/> definition for <see cref="TwilioAdapter"/>.
    /// </summary>
    public class TwilioAdapterBotComponent : BotComponent
    {
        /// <inheritdoc/>
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            if (TwilioAdapter.HasConfiguration(configuration))
            {
                // Components require the component configuration which is the subsection
                // assigned to the component. When the botbuilder-dotnet issue #5583 gets resolved, this could
                // change to the no-parameter overload.
                services.AddSingleton<IBotFrameworkHttpAdapter, TwilioAdapter>(sp => new TwilioAdapter(configuration));
            }
        }
    }
}
