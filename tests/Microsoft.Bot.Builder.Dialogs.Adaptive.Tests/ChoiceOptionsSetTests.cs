// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Input;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using Moq;
using Xunit;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Tests
{
    [CollectionDefinition("Dialogs.Adaptive")]
    public class ChoiceOptionsSetTests
    {
        private const string Template = "=${options.OrSeparator()}";
        private readonly Activity _activity = MessageFactory.Text("hi");

        [Fact]
        public void ContructorValidation()
        {
            Assert.NotNull(new ChoiceOptionsSet());
            
            Assert.NotNull(new ChoiceOptionsSet(Template));
        }

        [Fact]
        public async Task BindAsyncNullTemplate()
        {
            var choiceOptions = new ChoiceOptionsSet(null);
            var context = new TurnContext(new TestAdapter(), _activity);
            var dc = new DialogContext(new DialogSet(), context, new DialogState());

            var choiceFactory = await choiceOptions.BindAsync(dc);

            Assert.Equal(choiceOptions, choiceFactory);
        }

        [Fact]
        public async Task BindAsyncNullLanguageGenerator()
        {
            var choiceOptions = new ChoiceOptionsSet(Template);
            var context = new TurnContext(new TestAdapter(), _activity);
            var dc = new DialogContext(new DialogSet(), context, new DialogState());

            await Assert.ThrowsAsync<MissingMemberException>(async () => await choiceOptions.BindAsync(dc));
        }

        [Fact]
        public async Task BindAsyncReturnsLGResult()
        {
            var choiceOptions = new ChoiceOptionsSet(Template);
            var context = new TurnContext(new TestAdapter(), _activity);
            var lgResult = new ChoiceFactoryOptions();
            
            var mockLG = new Mock<LanguageGenerator>();
            mockLG.Setup(lg => lg.GenerateAsync(It.IsAny<DialogContext>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>())).ReturnsAsync(lgResult);

            var dc = new DialogContext(new DialogSet(), context, new DialogState());
            dc.Services.Add(mockLG.Object);

            var choiceFactory = await choiceOptions.BindAsync(dc);

            Assert.Equal(lgResult, choiceFactory);
        }

        [Fact]
        public async Task BindAsyncReturnsChoiceFactoryOptions()
        {
            var choiceOptions = new ChoiceOptionsSet(Template);
            var context = new TurnContext(new TestAdapter(), _activity);
            var lgResult = "[\",\",\" or\",\", or\",true]";

            var mockLG = new Mock<LanguageGenerator>();
            mockLG.Setup(lg => lg.GenerateAsync(It.IsAny<DialogContext>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>())).ReturnsAsync(lgResult);

            var dc = new DialogContext(new DialogSet(), context, new DialogState());
            dc.Services.Add(mockLG.Object);

            var choiceFactory = await choiceOptions.BindAsync(dc);

            Assert.Equal(",", choiceFactory.InlineSeparator);
            Assert.Equal(", or", choiceFactory.InlineOrMore);
            Assert.Equal(" or", choiceFactory.InlineOr);
            Assert.True(choiceFactory.IncludeNumbers);
        }

        [Fact]
        public async Task BindAsyncThrowsArgumentOutOfRangeException()
        {
            var choiceOptions = new ChoiceOptionsSet(Template);
            var context = new TurnContext(new TestAdapter(), _activity);
            var lgResult = "[\",\",\" or\"";

            var mockLG = new Mock<LanguageGenerator>();
            mockLG.Setup(lg => lg.GenerateAsync(It.IsAny<DialogContext>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>())).ReturnsAsync(lgResult);

            var dc = new DialogContext(new DialogSet(), context, new DialogState());
            dc.Services.Add(mockLG.Object);

            Assert.Null(await choiceOptions.BindAsync(dc));
        }

        [Fact]
        public async Task BindAsyncThrowsJsonReaderException()
        {
            var choiceOptions = new ChoiceOptionsSet(Template);
            var context = new TurnContext(new TestAdapter(), _activity);
            var lgResult = "[\",\",\" or\",\", or\",true]]";

            var mockLG = new Mock<LanguageGenerator>();
            mockLG.Setup(lg => lg.GenerateAsync(It.IsAny<DialogContext>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>())).ReturnsAsync(lgResult);

            var dc = new DialogContext(new DialogSet(), context, new DialogState());
            dc.Services.Add(mockLG.Object);

            Assert.Null(await choiceOptions.BindAsync(dc));
        }

        [Fact]
        public async Task BindAsyncReturnsNull()
        {
            var choiceOptions = new ChoiceOptionsSet(Template);
            var context = new TurnContext(new TestAdapter(), _activity);

            var mockLG = new Mock<LanguageGenerator>();
            mockLG.Setup(lg => lg.GenerateAsync(It.IsAny<DialogContext>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>())).ReturnsAsync(null);

            var dc = new DialogContext(new DialogSet(), context, new DialogState());
            dc.Services.Add(mockLG.Object);

            Assert.Null(await choiceOptions.BindAsync(dc));
        }
    }
}
