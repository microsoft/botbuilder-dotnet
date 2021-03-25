// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Builder.AI.QnA;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Adaptive;
using Microsoft.Bot.Builder.Dialogs.Declarative;

namespace Microsoft.Bot.Builder.Integration.Runtime.Component
{
    /// <summary>
    /// Retrieve the built-in enumeration of <see cref="BotComponent"/> instances.
    /// </summary>
    internal class BuiltInBotComponents
    {
        private static readonly List<BotComponent> _components = new List<BotComponent>()
        {
            new DialogsBotComponent(),
            new DeclarativeBotComponent(),
            new AdaptiveBotComponent(),
            new LanguageGenerationBotComponent(),
            new QnAMakerBotComponent(),
            new LuisBotComponent(),
        };

        /// <summary>
        /// Get available bot components.
        /// </summary>
        /// <returns>A collection of available bot plugins for the specified plugin name.</returns>
        public static IEnumerable<BotComponent> GetComponents()
        {
            return _components;
        }
    }
}
