// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Builder.AI.QnA;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Adaptive;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Teams;
using Microsoft.Bot.Builder.Dialogs.Declarative;

namespace Microsoft.Bot.Builder.Integration.Runtime
{
    /// <summary>
    /// Defines the standard set of component registrations to be applied within the bot runtime.
    /// </summary>
    internal static class ComponentRegistrations
    {
        /// <summary>
        /// Add standard component registrations to the bot runtime.
        /// </summary>
        internal static void Add()
        {
            ComponentRegistration.Add(new DialogsComponentRegistration());
            ComponentRegistration.Add(new DeclarativeComponentRegistration());
            ComponentRegistration.Add(new AdaptiveComponentRegistration());
            ComponentRegistration.Add(new LanguageGenerationComponentRegistration());
            ComponentRegistration.Add(new QnAMakerComponentRegistration());
            ComponentRegistration.Add(new LuisComponentRegistration());
            ComponentRegistration.Add(new TeamsComponentRegistration());
        }
    }
}
