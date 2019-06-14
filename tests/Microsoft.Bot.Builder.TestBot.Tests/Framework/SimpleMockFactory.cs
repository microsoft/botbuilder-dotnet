using System.Threading;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Recognizers.Text;
using Moq;

namespace Microsoft.BotBuilderSamples.Tests.Framework
{
    /// <summary>
    /// Contains utility methods for creating simple mock objects based on <see href="http://stackoverflow.com">moq</see>/>.
    /// </summary>
    public static class SimpleMockFactory
    {
        // Creates a simple mock dialog.
        public static Mock<T> CreateMockDialog<T>(object expectedResult = null, params object[] constructorParams)
            where T : Dialog
        {
            var mockDialog = new Mock<T>(constructorParams);
            var mockDialogNameTypeName = typeof(T).Name;
            mockDialog
                .Setup(x => x.BeginDialogAsync(It.IsAny<DialogContext>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
                .Returns(async (DialogContext dialogContext, object options, CancellationToken cancellationToken) =>
                {
                    await dialogContext.Context.SendActivityAsync($"{mockDialogNameTypeName} mock invoked", cancellationToken: cancellationToken);

                    return await dialogContext.EndDialogAsync(expectedResult, cancellationToken);
                });

            return mockDialog;
        }

        //public static Mock<T> CreateMockLuisRecognizer<T>(IRecognizerConvert returns)
        //    where T : LuisRecognizer
        //{
        //    var mockRecognizer = new Mock<T>();
        //    mockRecognizer
        //           .Setup(x => x.RecognizeAsync<T>(It.IsAny<ITurnContext>(), It.IsAny<CancellationToken>()))
        //           .Returns(() => Task.FromResult(returns));
        //    return mockRecognizer;
        //}
    }
}
