// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Adaptive;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Testing;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

[assembly: TestFramework("Microsoft.Bot.Builder.Startup", "Microsoft.Bot.Builder.AI.Luis.Tests")]

namespace Microsoft.Bot.Builder
{
    public class Startup : XunitTestFramework
    {
        public Startup(IMessageSink messageSink)
            : base(messageSink)
        {
            ComponentRegistration.Add(new DialogsComponentRegistration());
            ComponentRegistration.Add(new DeclarativeComponentRegistration());
            ComponentRegistration.Add(new AdaptiveComponentRegistration());
            ComponentRegistration.Add(new AdaptiveTestingComponentRegistration());
            ComponentRegistration.Add(new LuisComponentRegistration());
            ComponentRegistration.Add(new LanguageGenerationComponentRegistration());
        }

        public new void Dispose()
        {
            base.Dispose();
        }
    }
}
