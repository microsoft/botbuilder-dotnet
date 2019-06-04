using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Bot.Builder.Testing;
using Microsoft.BotBuilderSamples.Tests.Framework;
using Xunit;

namespace Microsoft.BotBuilderSamples.Tests.Dialogs
{
    public class DateResolverDialogTests
    {
        [Fact]
        public void Placeholder()
        {
            var sut = new DateResolverDialog(nameof(DateResolverDialog));
            var testClient = new DialogTestClient(sut);
        }
    }
}
