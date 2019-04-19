using System.Threading;
using Microsoft.Bot.Builder.Dialogs;
using Moq;

namespace Microsoft.BotBuilderSamples.Tests.Utils
{
    public static class DialogUtils
    {
        public static Mock<T> CreateMockDialog<T>(object expectedResult)
            where T : Dialog
        {
            var mockDialog = new Mock<T>();
            var mockDialogNameTypeName = typeof(T).Name;
            mockDialog.Setup(x => x.BeginDialogAsync(It.IsAny<DialogContext>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
                .Returns(async (DialogContext dialogContext, object options, CancellationToken cancellationToken) =>
                {
                    await dialogContext.Context.SendActivityAsync($"{mockDialogNameTypeName} mock invoked", cancellationToken: cancellationToken);

                    return await dialogContext.EndDialogAsync(expectedResult, cancellationToken);
                });
            return mockDialog;
        }
    }
}
