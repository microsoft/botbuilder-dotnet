// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder.Integration;

namespace Microsoft.Bot.Builder.Skills
{
    public static class BotAdapterExtensions
    {
        /// <summary>
        /// Enables the processing of Skills request from the ChannelApiController and registers skills.
        /// </summary>
        /// <param name="botAdapter">botAdapter to add skills to.</param>
        /// <param name="skills">optional skills to register.</param>
        /// <returns>botAdapter for fluent.</returns>
        public static BotAdapter UseSkills(this BotAdapter botAdapter, params SkillOptions[] skills)
        {
            botAdapter.Use(new ChannelApiMiddleware());
            if (skills != null && skills.Length > 0 && botAdapter is BotFrameworkAdapter botFrameworkAdapter)
            {
                // TODO, this is probably on BotAdapter, not BotFrameworkAdapter
                botFrameworkAdapter.Skills.AddRange(skills);
            }

            return botAdapter;
        }
    }
}
