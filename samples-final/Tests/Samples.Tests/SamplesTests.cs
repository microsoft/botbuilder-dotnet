using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Builder.Testing;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Samples.Tests
{
    [TestClass]
    public class SamplesTests
    {
        public SamplesTests()
        {
            TranscriptUtilities.DefaultTranscriptsRootFolder = @"..\..\..\..\..\..\transcripts";
        }

        [TestMethod]
        public async Task AspNetCoreEchoBotTest()
        {
            var activities = TranscriptUtilities.GetActivities($@"{nameof(SamplesTests)}\{nameof(AspNetCoreEchoBotTest)}.transcript");
            await TestBot<AspNetCore_EchoBot_With_State.EchoBot, AspNetCore_EchoBot_With_State.EchoState>(activities);
        }

        [TestMethod]
        public async Task ConsoleEchoBotTest()
        {
            var activities = TranscriptUtilities.GetActivities($@"{nameof(SamplesTests)}\{nameof(ConsoleEchoBotTest)}.transcript");
            await TestBot<Console_EchoBot_With_State.EchoBot, Console_EchoBot_With_State.EchoState>(activities);
        }

        [TestMethod]
        public async Task AspNetCoreSinglePromptBotTest()
        {
            var activities = TranscriptUtilities.GetActivities($@"{nameof(SamplesTests)}\{nameof(AspNetCoreSinglePromptBotTest)}.transcript");
            await TestBot<AspNetCore_Single_Prompts.SinglePromptBot, Dictionary<string, object>>(activities);
        }

        [TestMethod]
        public async Task AspNetCoreMultiplePromptsBotTest()
        {
            var activities = TranscriptUtilities.GetActivities($@"{nameof(SamplesTests)}\{nameof(AspNetCoreMultiplePromptsBotTest)}.transcript");
            await TestBot<AspNetCore_Multiple_Prompts.MultiplePromptsBot, AspNetCore_Multiple_Prompts.MultiplePromptsState>(activities);
        }

        [TestMethod]
        public async Task AspNetCoreConversationUpdateBotTest()
        {
            var activities = TranscriptUtilities.GetActivities($@"{nameof(SamplesTests)}\{nameof(AspNetCoreConversationUpdateBotTest)}.transcript");
            await TestBot<AspNetCore_ConversationUpdate_Bot.ConversationUpdateBot>(activities);
        }

        [TestMethod]
        public async Task AspNetCoreRichCardsBotTest()
        {
            var activities = TranscriptUtilities.GetActivities($@"{nameof(SamplesTests)}\{nameof(AspNetCoreRichCardsBotTest)}.transcript");
            await TestBot<AspNetCore_RichCards_Bot.RichCardsBot, Dictionary<string, object>>(activities);
        }

        private async Task TestBot<TBot, TState>(IEnumerable<IActivity> activities)
            where TBot : IBot, new()
            where TState : class, new()
        {
            await TestBot(new TBot(), new ConversationState<TState>(new MemoryStorage()), activities);
        }

        private async Task TestBot<TBot>(IEnumerable<IActivity> activities)
            where TBot : IBot, new()
        {
            await TestBot(new TBot(), activities);
        }

        private async Task TestBot(IBot bot, IEnumerable<IActivity> activities)
        {
            await TestBot(bot, null, activities);
        }

        private async Task TestBot(IBot bot, IMiddleware conversationState, IEnumerable<IActivity> activities)
        {
            var conversationReference = activities.First().GetConversationReference();

            // This adapter needs a custom conversation reference
            // to match the echobot behavior
            TestAdapter adapter = new TestAdapter(conversationReference);
            if (conversationState != null) adapter.Use(conversationState);

            var flow = new TestFlow(adapter, bot);

            await flow.Test(activities).StartTest();
        }
    }
}