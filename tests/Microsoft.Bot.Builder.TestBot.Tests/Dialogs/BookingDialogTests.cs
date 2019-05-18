// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.BotBuilderSamples.Tests.Utils;
using Microsoft.BotBuilderSamples.Tests.Utils.XUnit;
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
        [ClassData(typeof(BookingDialogTestsDataGenerator))]
        public async Task DialogFlowUseCases(TestDataObject testData)
        {
            var bookingTestData = testData.GetObject<BookingDialogTestData>();
            var sut = new BookingDialog();
            var testClient = new DialogTestClient(sut, Output, bookingTestData.BookingDetails);

            Output.WriteLine($"Use Case: {bookingTestData.TestCaseName}");
            for (var i = 0; i < bookingTestData.UtterancesAndReplies.GetLength(0); i++)
            {
                var reply = await testClient.SendAsync<IMessageActivity>(bookingTestData.UtterancesAndReplies[i, 0]);
                Assert.Equal(bookingTestData.UtterancesAndReplies[i, 1], reply.Text);
            }
        }

        [Fact]
        public void ShouldBeAbleToCancelAtAnyTime()
        {
            // TODO
        }
    }
}
