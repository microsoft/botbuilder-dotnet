// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text;
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
                services.AddSingleton<TwilioAdapter>();
            }
        }
    }
}
