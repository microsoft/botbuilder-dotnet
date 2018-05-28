// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Builder.Prompts;
using Microsoft.Bot.Builder.Testing;
using Microsoft.Recognizers.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Dialogs.Tests
{
    [TestClass]
    public class DialogDateTimePromptTests
    {
        [TestMethod]
        public async Task BasicDateTimePrompt()
        {
            TestAdapter adapter = new TestAdapter()
                .Use(new ConversationState<Dictionary<string, object>>(new MemoryStorage()));

            await new TestFlow(adapter, async (turnContext) =>
            {
                var state = ConversationState<Dictionary<string, object>>.Get(turnContext);
                var prompt = new DateTimePrompt(Culture.English);

                var dialogCompletion = await prompt.Continue(turnContext, state);
                if (!dialogCompletion.IsActive && !dialogCompletion.IsCompleted)
                {
                    await prompt.Begin(turnContext, state, new PromptOptions { PromptString = "What date would you like?" });
                }
                else if (dialogCompletion.IsCompleted)
                {
                    var dateTimeResult = (DateTimeResult)dialogCompletion.Result;
                    var resolution = dateTimeResult.Resolution.First();
                    var reply = $"Timex:'{resolution.Timex}' Value:'{resolution.Value}'";
                    await turnContext.SendActivity(reply);
                }
            })
            .Send("hello")
            .AssertReply("What date would you like?")
            .Send("5th December 2018 at 9am")
            .AssertReply("Timex:'2018-12-05T09' Value:'2018-12-05 09:00:00'")
            .StartTest();
        }
    }
}
