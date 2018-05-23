// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Builder.Testing;
using Microsoft.Recognizers.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Microsoft.Bot.Builder.Prompts.PromptValidatorEx;

namespace Microsoft.Bot.Builder.Prompts.Tests
{
    [TestClass]
    [TestCategory("Prompts")]
    [TestCategory("DateTime Prompts")]

    public class DateTimePromptTests
    {
        [TestMethod]
        public async Task DateTimePrompt_ShouldSendPrompt()
        {
            await new TestFlow(new TestAdapter(), async (context) =>
            {
                var dateTimePrompt = new DateTimePrompt(Culture.English);
                await dateTimePrompt.Prompt(context, "What date would you like?");
            })
            .Send("hello")
            .AssertReply("What date would you like?")
            .StartTest();
        }

        [TestMethod]
        public async Task DateTimePrompt_ShouldRecognizeDateTime_Value()
        {
            var adapter = new TestAdapter()
                .Use(new ConversationState<TestState>(new MemoryStorage()));

            await new TestFlow(adapter, async (context) =>
            {
                var state = ConversationState<TestState>.Get(context);

                var dateTimePrompt = new DateTimePrompt(Culture.English);
                if (!state.InPrompt)
                {
                    state.InPrompt = true;
                    await dateTimePrompt.Prompt(context, "What date would you like?");
                }
                else
                {
                    var dateTimeResult = await dateTimePrompt.Recognize(context);
                    if (dateTimeResult.Succeeded())
                    {
                        var resolution = dateTimeResult.Resolution.First();
                        var reply = $"Timex:'{resolution.Timex}' Value:'{resolution.Value}'";
                        await context.SendActivity(reply);
                    }
                    else
                    {
                        await context.SendActivity(dateTimeResult.Status.ToString());
                    }
                }
            })
            .Send("hello")
            .AssertReply("What date would you like?")
            .Send("5th December 2018 at 9am")
            .AssertReply("Timex:'2018-12-05T09' Value:'2018-12-05 09:00:00'")
            .StartTest();
        }

        [TestMethod]
        public async Task DateTimePrompt_ShouldRecognizeDateTime_Range()
        {
            var adapter = new TestAdapter()
                .Use(new ConversationState<TestState>(new MemoryStorage()));

            await new TestFlow(adapter, async (context) =>
            {
                var state = ConversationState<TestState>.Get(context);

                var dateTimePrompt = new DateTimePrompt(Culture.English);
                if (!state.InPrompt)
                {
                    state.InPrompt = true;
                    await dateTimePrompt.Prompt(context, "What window would you like to book?");
                }
                else
                {
                    var dateTimeResult = await dateTimePrompt.Recognize(context);
                    if (dateTimeResult.Succeeded())
                    {
                        var resolution = dateTimeResult.Resolution.First();
                        var reply = $"Timex:'{resolution.Timex}'";
                        await context.SendActivity(reply);
                    }
                    else
                    {
                        await context.SendActivity(dateTimeResult.Status.ToString());
                    }
                }
            })
            .Send("hello")
            .AssertReply("What window would you like to book?")
            .Send("4pm wednesday to 3pm Saturday")
            .AssertReply("Timex:'(XXXX-WXX-3T16,XXXX-WXX-6T15,PT71H)'")
            .StartTest();
        }

        [TestMethod]
        public async Task DateTimePrompt_ShouldRecognizeDateTime_StartEnd()
        {
            var adapter = new TestAdapter()
                .Use(new ConversationState<TestState>(new MemoryStorage()));

            await new TestFlow(adapter, async (context) =>
            {
                var state = ConversationState<TestState>.Get(context);

                var dateTimePrompt = new DateTimePrompt(Culture.English);
                if (!state.InPrompt)
                {
                    state.InPrompt = true;
                    await dateTimePrompt.Prompt(context, "What month are you interested in?");
                }
                else
                {
                    var dateTimeResult = await dateTimePrompt.Recognize(context);
                    if (dateTimeResult.Succeeded())
                    {
                        var resolution = dateTimeResult.Resolution.First();
                        var reply = $"Timex:'{resolution.Timex}' Start:'{resolution.Start}' End:'{resolution.End}'";
                        await context.SendActivity(reply);
                    }
                    else
                    {
                        await context.SendActivity(dateTimeResult.Status.ToString());
                    }
                }
            })
            .Send("hello")
            .AssertReply("What month are you interested in?")
            .Send("May 1967")
            .AssertReply("Timex:'1967-05' Start:'1967-05-01' End:'1967-06-01'")
            .StartTest();
        }

        [TestMethod]
        public async Task DateTimePrompt_ShouldRecognizeDateTime_CustomValidator()
        {
            var adapter = new TestAdapter()
                .Use(new ConversationState<TestState>(new MemoryStorage()));

            PromptValidator<DateTimeResult> validator = (context, result) =>
            {
                result.Status = PromptStatus.NotRecognized;
                return Task.CompletedTask;
            };

            await new TestFlow(adapter, async (context) =>
            {
                var state = ConversationState<TestState>.Get(context);

                var dateTimePrompt = new DateTimePrompt(Culture.English, validator);
                if (!state.InPrompt)
                {
                    state.InPrompt = true;
                    await dateTimePrompt.Prompt(context, "What date would you like?");
                }
                else
                {
                    var dateTimeResult = await dateTimePrompt.Recognize(context);
                    if (dateTimeResult.Succeeded())
                    {
                        var resolution = dateTimeResult.Resolution.First();
                        var reply = $"Timex:'{resolution.Timex}' Value:'{resolution.Value}'";
                        await context.SendActivity(reply);
                    }
                    else
                    {
                        await context.SendActivity(dateTimeResult.Status.ToString());
                    }
                }
            })
            .Send("hello")
            .AssertReply("What date would you like?")
            .Send("5th December 2018 at 9am")
            .AssertReply("NotRecognized")
            .StartTest();
        }
    }
}
