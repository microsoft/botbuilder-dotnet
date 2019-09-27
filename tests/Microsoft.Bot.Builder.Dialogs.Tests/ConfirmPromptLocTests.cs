// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Microsoft.Bot.Builder.Dialogs.Prompts.PromptCultureModels;

namespace Microsoft.Bot.Builder.Dialogs.Tests
{
    [TestClass]
    public class ConfirmPromptLocTests
    {
        [TestMethod]
        [DataRow(null, Culture.Dutch, "(1) Ja of (2) Nee", "Ja", "1")]
        [DataRow(null, Culture.Dutch, "(1) Ja of (2) Nee", "Nee", "0")]
        [DataRow(null, Culture.Spanish, "(1) Sí o (2) No", "Sí", "1")]
        [DataRow(null, Culture.Spanish, "(1) Sí o (2) No", "No", "0")]
        [DataRow(null, Culture.English, "(1) Yes or (2) No", "Yes", "1")]
        [DataRow(null, Culture.English, "(1) Yes or (2) No", "No", "0")]
        [DataRow(null, Culture.French, "(1) Oui ou (2) Non", "Oui", "1")]
        [DataRow(null, Culture.French, "(1) Oui ou (2) Non", "Non", "0")]
        [DataRow(null, Culture.German, "(1) Ja oder (2) Nein", "Ja", "1")]
        [DataRow(null, Culture.German, "(1) Ja oder (2) Nein", "Nein", "0")]
        [DataRow(null, Culture.Japanese, "(1) はい または (2) いいえ", "はい", "1")]
        [DataRow(null, Culture.Japanese, "(1) はい または (2) いいえ", "いいえ", "0")]
        [DataRow(null, Culture.Portuguese, "(1) Sim ou (2) Não", "Sim", "1")]
        [DataRow(null, Culture.Portuguese, "(1) Sim ou (2) Não", "Não", "0")]
        [DataRow(null, Culture.Chinese, "(1) 是的 要么 (2) 不", "是的", "1")]
        [DataRow(null, Culture.Chinese, "(1) 是的 要么 (2) 不", "不", "0")]
        public async Task ConfirmPrompt_Activity_Locale_Default(string activityLocale, string defaultLocale, string prompt, string utterance, string expectedResponse)
        {
            await ConfirmPrompt_Locale_Impl(activityLocale, defaultLocale, prompt, utterance, expectedResponse);
        }

        [TestMethod]
        [DataRow(null, null, "(1) Yes or (2) No", "Yes", "1")]
        [DataRow(null, "", "(1) Yes or (2) No", "Yes", "1")]
        [DataRow(null, "not-supported", "(1) Yes or (2) No", "Yes", "1")]
        public async Task ConfirmPrompt_Activity_Locale_Illegal_Default(string activityLocale, string defaultLocale, string prompt, string utterance, string expectedResponse)
        {
            await ConfirmPrompt_Locale_Impl(activityLocale, defaultLocale, prompt, utterance, expectedResponse);
        }

        [TestMethod]
        [DataRow(null, Culture.Dutch, "(1) Ja of (2) Nee", "1", "1")]
        [DataRow(null, Culture.Dutch, "(1) Ja of (2) Nee", "2", "0")]
        [DataRow(null, Culture.Spanish, "(1) Sí o (2) No", "1", "1")]
        [DataRow(null, Culture.Spanish, "(1) Sí o (2) No", "2", "0")]
        [DataRow(null, Culture.English, "(1) Yes or (2) No", "1", "1")]
        [DataRow(null, Culture.English, "(1) Yes or (2) No", "2", "0")]
        [DataRow(null, Culture.French, "(1) Oui ou (2) Non", "1", "1")]
        [DataRow(null, Culture.French, "(1) Oui ou (2) Non", "2", "0")]
        [DataRow(null, Culture.German, "(1) Ja oder (2) Nein", "1", "1")]
        [DataRow(null, Culture.German, "(1) Ja oder (2) Nein", "2", "0")]
        [DataRow(null, Culture.Japanese, "(1) はい または (2) いいえ", "1", "1")]
        [DataRow(null, Culture.Japanese, "(1) はい または (2) いいえ", "2", "0")]
        [DataRow(null, Culture.Portuguese, "(1) Sim ou (2) Não", "1", "1")]
        [DataRow(null, Culture.Portuguese, "(1) Sim ou (2) Não", "2", "0")]
        [DataRow(null, Culture.Chinese, "(1) 是的 要么 (2) 不", "1", "1")]
        [DataRow(null, Culture.Chinese, "(1) 是的 要么 (2) 不", "2", "0")]
        public async Task ConfirmPrompt_Activity_Locale_Default_Number(string activityLocale, string defaultLocale, string prompt, string utterance, string expectedResponse)
        {
            await ConfirmPrompt_Locale_Impl(activityLocale, defaultLocale, prompt, utterance, expectedResponse);
        }

        [TestMethod]
        [DataRow(null, null, "(1) Yes or (2) No", "1", "1")]
        [DataRow(null, "", "(1) Yes or (2) No", "1", "1")]
        [DataRow(null, "not-supported", "(1) Yes or (2) No", "1", "1")]
        public async Task ConfirmPrompt_Activity_Locale_Illegal_Default_Number(string activityLocale, string defaultLocale, string prompt, string utterance, string expectedResponse)
        {
            await ConfirmPrompt_Locale_Impl(activityLocale, defaultLocale, prompt, utterance, expectedResponse);
        }

        [TestMethod]
        [DataRow(Culture.Dutch, null, "(1) Ja of (2) Nee", "Ja", "1")]
        [DataRow(Culture.Dutch, null, "(1) Ja of (2) Nee", "Nee", "0")]
        [DataRow(Culture.Spanish, null, "(1) Sí o (2) No", "Sí", "1")]
        [DataRow(Culture.Spanish, null, "(1) Sí o (2) No", "No", "0")]
        [DataRow(Culture.English, null, "(1) Yes or (2) No", "Yes", "1")]
        [DataRow(Culture.English, null, "(1) Yes or (2) No", "No", "0")]
        [DataRow(Culture.French, null, "(1) Oui ou (2) Non", "Oui", "1")]
        [DataRow(Culture.French, null, "(1) Oui ou (2) Non", "Non", "0")]
        [DataRow(Culture.German, null, "(1) Ja oder (2) Nein", "Ja", "1")]
        [DataRow(Culture.German, null, "(1) Ja oder (2) Nein", "Nein", "0")]
        [DataRow(Culture.Japanese, null, "(1) はい または (2) いいえ", "はい", "1")]
        [DataRow(Culture.Japanese, null, "(1) はい または (2) いいえ", "いいえ", "0")]
        [DataRow(Culture.Portuguese, null, "(1) Sim ou (2) Não", "Sim", "1")]
        [DataRow(Culture.Portuguese, null, "(1) Sim ou (2) Não", "Não", "0")]
        [DataRow(Culture.Chinese, null, "(1) 是的 要么 (2) 不", "是的", "1")]
        [DataRow(Culture.Chinese, null, "(1) 是的 要么 (2) 不", "不", "0")]
        public async Task ConfirmPrompt_Activity_Locale_Activity(string activityLocale, string defaultLocale, string prompt, string utterance, string expectedResponse)
        {
            await ConfirmPrompt_Locale_Impl(activityLocale, defaultLocale, prompt, utterance, expectedResponse);
        }

        [TestMethod]
        [DataRow(null, null, "(1) Yes or (2) No", "Yes", "1")]
        [DataRow("", null, "(1) Yes or (2) No", "Yes", "1")]
        [DataRow("not-supported", null, "(1) Yes or (2) No", "Yes", "1")]
        public async Task ConfirmPrompt_Activity_Locale_Illegal_Activity(string activityLocale, string defaultLocale, string prompt, string utterance, string expectedResponse)
        {
            await ConfirmPrompt_Locale_Impl(activityLocale, defaultLocale, prompt, utterance, expectedResponse);
        }

        [DataTestMethod]
        [DynamicData(nameof(GetLocaleVariationTest), DynamicDataSourceType.Method)]
        public async Task ConfirmPrompt_Locale_Variations(string activityLocale, string defaultLocale, string prompt, string utterance, string expectedResponse)
        {
            await ConfirmPrompt_Locale_Impl(activityLocale, defaultLocale, prompt, utterance, expectedResponse);
        }

        private async Task ConfirmPrompt_Locale_Impl(string activityLocale, string defaultLocale, string prompt, string utterance, string expectedResponse)
        {
            var convoState = new ConversationState(new MemoryStorage());
            var dialogState = convoState.CreateProperty<DialogState>("dialogState");

            var adapter = new TestAdapter()
                .Use(new AutoSaveStateMiddleware(convoState));

            // Create new DialogSet.
            var dialogs = new DialogSet(dialogState);

            // Prompt should default to English if locale is a non-supported value
            dialogs.Add(new ConfirmPrompt("ConfirmPrompt", defaultLocale: defaultLocale));

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                turnContext.Activity.Locale = activityLocale;

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

        /// <summary>
        /// Generates an Enumerable of variations on all supported locales.
        /// </summary>
#pragma warning disable SA1204 // Static elements should appear before instance elements
        private static IEnumerable<object[]> GetLocaleVariationTest()
        {
            var testLocales = new TestLocale[]
            {
                new TestLocale(Chinese, "(1) 是的 要么 (2) 不", "是的", "不"),
                new TestLocale(Danish, "(1) Ja of (2) Nej", "Ja", "Nej"),
                new TestLocale(Dutch, "(1) Ja of (2) Nee", "Ja", "Nee"),
                new TestLocale(English, "(1) Yes or (2) No", "Yes", "No"),
                new TestLocale(French, "(1) Oui ou (2) Non", "Oui", "Non"),
                new TestLocale(German, "(1) Ja oder (2) Nein", "Ja", "Nein"),
                new TestLocale(Japanese, "(1) はい または (2) いいえ", "はい", "いいえ"),
                new TestLocale(Portuguese, "(1) Sim ou (2) Não", "Sim", "Não"),
                new TestLocale(Spanish, "(1) Sí o (2) No", "Sí", "No"),
            };

            foreach (var locale in testLocales)
            {
                yield return new object[] { locale.ValidLocale, locale.ValidLocale, locale.ExpectedPrompt, locale.InputThatResultsInOne, "1" };
                yield return new object[] { locale.ValidLocale, locale.ValidLocale, locale.ExpectedPrompt, locale.InputThatResultsInZero, "0" };

                yield return new object[] { locale.CapEnding, locale.CapEnding, locale.ExpectedPrompt, locale.InputThatResultsInOne, "1" };
                yield return new object[] { locale.CapEnding, locale.CapEnding, locale.ExpectedPrompt, locale.InputThatResultsInZero, "0" };

                yield return new object[] { locale.TitleEnding, locale.TitleEnding, locale.ExpectedPrompt, locale.InputThatResultsInOne, "1" };
                yield return new object[] { locale.TitleEnding, locale.TitleEnding, locale.ExpectedPrompt, locale.InputThatResultsInZero, "0" };

                yield return new object[] { locale.CapTwoLetter, locale.CapTwoLetter, locale.ExpectedPrompt, locale.InputThatResultsInOne, "1" };
                yield return new object[] { locale.CapTwoLetter, locale.CapTwoLetter, locale.ExpectedPrompt, locale.InputThatResultsInZero, "0" };

                yield return new object[] { locale.LowerTwoLetter, locale.LowerTwoLetter, locale.ExpectedPrompt, locale.InputThatResultsInOne, "1" };
                yield return new object[] { locale.LowerTwoLetter, locale.LowerTwoLetter, locale.ExpectedPrompt, locale.InputThatResultsInZero, "0" };
            }
        }
    }
}
