// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core.Skills;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Builder
{
    // TODO: deleted this. Gabo
    public static class SkillsDependencyInjectionExtensions
    {
        /// <summary>
        /// Configures the web app to handle bot skills.
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder"/> hosting the bot.</param>
        /// <returns>The <see cref="IApplicationBuilder"/> instance.</returns>
        public static IApplicationBuilder UseBotSkills(this IApplicationBuilder app)
        {
            // Force the resolution of the skills server so the SkillAdapter can register the middleware.
            // This will also trigger the creation of the BotAdapter.
            var skillsServer = app.ApplicationServices.GetService<BotFrameworkSkillHttpHostAdapter>();

            if (skillsServer == null)
            {
                throw new NullReferenceException($"Unable to resolve service for type {typeof(BotFrameworkSkillHttpHostAdapter)}. Ensure the service is registered in Startup.cs.");
            }

            return app;
        }
    }
}
