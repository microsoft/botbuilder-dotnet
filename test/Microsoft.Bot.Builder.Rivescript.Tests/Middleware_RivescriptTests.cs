using Microsoft.Bot.Builder.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using static Microsoft.Bot.Builder.Rivescript.Tests.RivescriptTestUtilities;

namespace Microsoft.Bot.Builder.Rivescript.Tests
{
    [TestClass]
    [TestCategory("Middleware")]
    [TestCategory("RiveScript")]
    [TestCategory("RiveScript - Basic")]
    public class Middleware_RivescriptTests
    {
        [TestCleanup]
        public void Cleanup()
        {
            CleanupTempFiles();
        }

        [TestMethod]
        public async Task Middleware_Rivescript_HelloBot()
        {
            // RiveScript Sample file, taken from their 
            // tutorial at: https://www.rivescript.com/docs/tutorial
            string fileName = CreateTempFile(
                         @"! version = 2.0

                           +hello bot
                           -Hello, human!");

            TestConnector connector = CreateSimpleRivescriptBot(fileName);

            await connector
                .Send("hello bot").AssertReply("Hello, human!")
                .StartTest();
        }

        [TestMethod]
        public async Task Middleware_Rivescript_Trigger()
        {
            // RiveScript Sample file, taken from their tutorial at:
            // https://www.rivescript.com/docs/tutorial

            string fileName = CreateTempFile(
                @"! version = 2.0
                    
                   + my name is *
                   - Nice to meet you, <star1>!");

            TestConnector connector = CreateSimpleRivescriptBot(fileName);
            
            await connector
                .Send("my name is giskard").AssertReply("Nice to meet you, giskard!")
                .StartTest();
        }
    }
}
