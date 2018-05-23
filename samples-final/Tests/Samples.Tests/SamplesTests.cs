using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Console_EchoBot_With_State;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Samples.Tests
{
    [TestClass]
    // [Transcript("https://raw.githubusercontent.com/Microsoft/botbuilder-transcripts/tree/master/SamplesTests")]
    public class SamplesTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        // [TranscriptFile("ConsoleEchoBotTest.chat")]
        public async Task ConsoleEchoBotTest()
        {
            // TODO: This logic should be moved to another artifact like an attribute
            var transcriptsRootFolder = Environment.GetEnvironmentVariable("TranscriptsRootFolder") ?? @"..\..\..\..\..\transcripts";
            var directory = Path.Combine(transcriptsRootFolder, TestContext.FullyQualifiedTestClassName.Split('.').Last());
            var fileName = $"{TestContext.TestName}.transcript";
            var path = Path.Combine(directory, fileName);

            var activities = TranscriptUtilities.GetActivities(path);
            var conversationReference = activities.First().GetConversationReference();

            // This adapter needs a custom conversation reference
            // to match the echobot behavior
            TestAdapter adapter = new TestAdapter(conversationReference)
                .Use(new ConversationState<EchoState>(new MemoryStorage()));

            var flow = new TestFlow(adapter, new EchoBot());

            await flow.Test(activities, (expected, actual) => {
                Assert.AreEqual(expected.Type, actual.Type, "Type should match");
                if (expected.Type == ActivityTypes.Message)
                {
                    Assert.AreEqual(expected.AsMessageActivity().Text, actual.AsMessageActivity().Text);
                }
            }).StartTest();
        }
    }
}
