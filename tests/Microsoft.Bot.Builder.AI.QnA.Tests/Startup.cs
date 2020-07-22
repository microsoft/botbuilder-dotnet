// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder.AI.QnA;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Adaptive;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Testing;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Xunit;

namespace Microsoft.Bot.Builder
{
    public class Startup
    {
        public static void Initialize()
        {
            ComponentRegistration.Add(new DeclarativeComponentRegistration());
            ComponentRegistration.Add(new AdaptiveComponentRegistration());
            ComponentRegistration.Add(new AdaptiveTestingComponentRegistration());
            ComponentRegistration.Add(new LanguageGenerationComponentRegistration());
            ComponentRegistration.Add(new QnAMakerComponentRegistration());
        }
    }
}
