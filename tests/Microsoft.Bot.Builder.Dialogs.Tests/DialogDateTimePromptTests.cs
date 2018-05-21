// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Builder.Core.Extensions.Tests;
using Microsoft.Bot.Builder.Prompts;
using Microsoft.Recognizers.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Dialogs.Tests
{
    [TestClass]
    public class DialogDateTimePromptTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public async Task BasicDateTimePrompt()
        {
            var activities = TranscriptUtilities.GetFromTestContext(TestContext);

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
            .Test(activities)
            .StartTest();
        }
    }
}
