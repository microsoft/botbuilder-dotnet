// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Dialogs.Tests
{
    [TestClass]
    public class ConfirmPromptLocTests
    {
        [TestMethod]
        [DataRow(Culture.Dutch, "(1) Ja of (2) Nee", "Ja", "1")]
        [DataRow(Culture.Dutch, "(1) Ja of (2) Nee", "Nee", "0")]
        [DataRow(Culture.Spanish, "(1) Sí o (2) No", "Sí", "1")]
        [DataRow(Culture.Spanish, "(1) Sí o (2) No", "No", "0")]
        [DataRow(Culture.English, "(1) Yes or (2) No", "Yes", "1")]
        [DataRow(Culture.English, "(1) Yes or (2) No", "No", "0")]
        //[DataRow(Culture.French, "(1) Oui ou (2) Non", "Oui", "1")]
        //[DataRow(Culture.French, "(1) Oui ou (2) Non", "Non", "0")]
        //[DataRow(Culture.German, "(1) Ja oder (2) Nein", "Ja", "1")]
        //[DataRow(Culture.German, "(1) Ja oder (2) Nein", "Nein", "0")]
        [DataRow(Culture.Japanese, "(1) はい または (2) いいえ", "はい", "1")]
        [DataRow(Culture.Japanese, "(1) はい または (2) いいえ", "いいえ", "0")]
        [DataRow(Culture.Portuguese, "(1) Sim ou (2) Não", "Sim", "1")]
        [DataRow(Culture.Portuguese, "(1) Sim ou (2) Não", "Não", "0")]
        //[DataRow(Culture.Chinese, "(1) 是的 要么 (2) 不", "是的", "1")]
        //[DataRow(Culture.Chinese, "(1) 是的 要么 (2) 不", "不", "0")]
        public async Task ConfirmPrompt_Locale(string locale, string prompt, string utterance, string expectedResponse)
        {
            var convoState = new ConversationState(new MemoryStorage());
            var dialogState = convoState.CreateProperty<DialogState>("dialogState");

            var adapter = new TestAdapter()
                .Use(new AutoSaveStateMiddleware(convoState));

            // Create new DialogSet.
            var dialogs = new DialogSet(dialogState);
            dialogs.Add(new ConfirmPrompt("ConfirmPrompt", defaultLocale: locale));

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var dc = await dialogs.CreateContextAsync(turnContext, cancellationToken);

                var results = await dc.ContinueDialogAsync(cancellationToken);
                if (results.Status == DialogTurnStatus.Empty)
                {
                    await dc.PromptAsync("ConfirmPrompt", new PromptOptions { Prompt = new Activity { Type = ActivityTypes.Message, Text = "Prompt." } }, cancellationToken);
                }
                else if (results.Status == DialogTurnStatus.Complete)
                {
                    if ((bool)results.Result)
                    {
                        await turnContext.SendActivityAsync(MessageFactory.Text("1"), cancellationToken);
                    }
                    else
                    {
                        await turnContext.SendActivityAsync(MessageFactory.Text("0"), cancellationToken);
                    }
                }
            })
            .Send("hello")
            .AssertReply("Prompt. " + prompt)
            .Send(utterance)
            .AssertReply(expectedResponse)
            .StartTestAsync();
        }
    }
}
