// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace Microsoft.Bot.Builder.Integration.AspNet.Core.Skills
{
    public static class BotAdapterExtensions
    {
        /// <summary>
        /// Enables the processing of Skills request from the ChannelApiController and registers skills.
        /// </summary>
        /// <param name="botAdapter">botAdapter to add skills to.</param>
        /// <param name="callbackUri">The Uri that will be used by the skills to communicate back with the bot.</param>
        /// <param name="skills">optional skills to register.</param>
        /// <returns>botAdapter for fluent.</returns>
        public static BotAdapter UseSkills(this BotAdapter botAdapter, Uri callbackUri, SkillOptions[] skills)
        {
            botAdapter.Use(new ChannelApiMiddleware());
            if (skills != null && skills.Length > 0 && botAdapter is BotFrameworkAdapter botFrameworkAdapter)
            {
                botFrameworkAdapter.SkillsCallbackUri = callbackUri;

                // TODO, this is probably on BotAdapter, not BotFrameworkAdapter
                botFrameworkAdapter.Skills.AddRange(skills);
            }

            return botAdapter;
        }
    }
}
