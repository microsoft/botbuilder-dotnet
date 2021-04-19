using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Adaptive;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Testing;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

[assembly: TestFramework("Microsoft.Bot.Builder.Startup", "Microsoft.Bot.Builder.Dialogs.Tests")]

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
            ComponentRegistration.Add(new LanguageGenerationComponentRegistration());
        }
    }
}
