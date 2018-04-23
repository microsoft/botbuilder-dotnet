// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Builder.Prompts;
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
                var dialogs = new DialogSet();
                dialogs.Add("test-prompt", new DateTimePrompt(Culture.English));

                var state = ConversationState<Dictionary<string, object>>.Get(turnContext);
                var dc = dialogs.CreateContext(turnContext, state);

                await dc.Continue();
                var dialogResult = dc.DialogResult;

                if (!dialogResult.Active)
                {
                    var result = dialogResult.Result as DateTimeResult;
                    if (result != null)
                    {
                        var resolution = $"Timex:'{result.Timex}' Value:'{result.Value}'";
                        await turnContext.SendActivity(resolution);
                    }
                    else
                    {
                        await dc.Prompt("test-prompt", "What date would you like?");
                    }
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
