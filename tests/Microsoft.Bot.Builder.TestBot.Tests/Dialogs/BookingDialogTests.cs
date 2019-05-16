// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.BotBuilderSamples.Tests.Utils;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.BotBuilderSamples.Tests.Dialogs
{
    public class BookingDialogTests : DialogTestsBase
    {
        public BookingDialogTests(ITestOutputHelper output)
            : base(output)
        {
        }

        [Fact]
        public void DialogConstructor()
        {
            var sut = new BookingDialog();

            Assert.Equal("BookingDialog", sut.Id);
            Assert.IsType<TextPrompt>(sut.FindDialog("TextPrompt"));
            Assert.IsType<ConfirmPrompt>(sut.FindDialog("ConfirmPrompt"));
            Assert.IsType<DateResolverDialog>(sut.FindDialog("DateResolverDialog"));
            Assert.IsType<WaterfallDialog>(sut.FindDialog("WaterfallDialog"));
        }

        [Theory]
        [ClassData(typeof(BookingDialogTestsDialogFlowData))]
        public async Task DialogFlowUseCases(string useCaseName, BookingDetails inputBookingInfo, string[,] utterancesAndReplies)
        {
            var sut = new BookingDialog();
            var testBot = new DialogsTestBot(sut, Output, inputBookingInfo);

            Output.WriteLine($"Use Case: {useCaseName}");
            for (var i = 0; i < utterancesAndReplies.GetLength(0); i++)
            {
                var reply = await testBot.SendAsync<IMessageActivity>(utterancesAndReplies[i, 0]);
                Assert.Equal(utterancesAndReplies[i, 1], reply.Text);
            }
        }

        [Fact]
        public void ShouldBeAbleToCancelAtAnyTime()
        {
            // TODO
        }
    }
}
