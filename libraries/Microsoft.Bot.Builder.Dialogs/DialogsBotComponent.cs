// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder.Dialogs.Memory;
using Microsoft.Bot.Builder.Dialogs.Memory.PathResolvers;
using Microsoft.Bot.Builder.Dialogs.Memory.Scopes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// Bot component for bot Dialogs.
    /// </summary>
    public class DialogsBotComponent : BotComponent
    {
        /// <inheritdoc/>
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration, ILogger logger)
        {
            // Register memory scopes
            services.AddSingleton<MemoryScope, TurnMemoryScope>();
            services.AddSingleton<MemoryScope>(sp => new SettingsMemoryScope(configuration));
            services.AddSingleton<MemoryScope, DialogMemoryScope>();
            services.AddSingleton<MemoryScope, DialogContextMemoryScope>();
            services.AddSingleton<MemoryScope, DialogClassMemoryScope>();
            services.AddSingleton<MemoryScope, ClassMemoryScope>();
            services.AddSingleton<MemoryScope, ThisMemoryScope>();
            services.AddSingleton<MemoryScope, ConversationMemoryScope>();
            services.AddSingleton<MemoryScope, UserMemoryScope>();

            // Register path resolvers
            services.AddSingleton<IPathResolver, DollarPathResolver>();
            services.AddSingleton<IPathResolver, HashPathResolver>();
            services.AddSingleton<IPathResolver, AtAtPathResolver>();
            services.AddSingleton<IPathResolver, AtPathResolver>();
            services.AddSingleton<IPathResolver, PercentPathResolver>();
        }
    }
}
