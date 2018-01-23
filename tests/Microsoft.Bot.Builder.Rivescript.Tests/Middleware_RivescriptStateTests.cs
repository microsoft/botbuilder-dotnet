using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;
using static Microsoft.Bot.Builder.Rivescript.Tests.RivescriptTestUtilities;

namespace Microsoft.Bot.Builder.Rivescript.Tests
{
    [TestClass]
    [TestCategory("Middleware")]
    [TestCategory("RiveScript")]
    [TestCategory("RiveScript - State")]
    public class Middleware_RivescriptStateTests
    {
        [TestCleanup]
        public void Cleanup()
        {
            CleanupTempFiles();
        }

        [TestMethod]
        public async Task Middleware_Rivescript_StateFromBotToRivescript()
        {
            string name = Guid.NewGuid().ToString();

            string fileName = CreateTempFile(
                         @"! version = 2.0

                           +hello bot
                           -Test <get name>");

            var adapter = new TestAdapter();

            Bot bot = new Bot(adapter)
                .Use(new InjectState((context) => {
                    var dict = RivescriptMiddleware.StateDictionary(context);
                    dict["name"] = name; 
                }))
                .Use(new RivescriptMiddleware(fileName));

            await adapter
                .Send("Hello bot").AssertReply("Test " + name)
                .StartTest();
        }

        [TestMethod]
        public async Task Middleware_Rivescript_StateFromRivescriptToBot()
        {
            //Note: The dictionary coming back from the C# Rivescript implementation
            // eats the "-" in a GUID. This means if we send in "abcde-12345" we get
            // back "abcde12345". This behavior is confirmed to be scoped to the 
            // Rivescript implementation, and not caused by the BotBuilder Middleware.                         
            // To work around this, this test - which is just testing the BotBuilder 
            // code - doesn't use any "-". 
            string uglyGuid = Guid.NewGuid().ToString("N"); 
            bool validationRan = false;

            string fileName = CreateTempFile(
                         @"! version = 2.0

                           + value is *
                           - <set test=<star>>value is <get test>");

            var adapter = new TestAdapter();

            Bot bot = new Bot(adapter)                                
                .Use(new RivescriptMiddleware(fileName))
                .Use(new ValidateState((context) =>
                    {
                        var dict = RivescriptMiddleware.StateDictionary(context); 
                        Assert.IsTrue(
                            dict["test"] == uglyGuid,
                            $"Incorrect value. Expected '{uglyGuid}', found '{dict["test"]}'");
                        validationRan = true;
                    })
                );

            await adapter
                .Send("value is " + uglyGuid).AssertReply("value is " + uglyGuid)
                .StartTest();

            // Make sure the state validator actually ran. 
            Assert.IsTrue(validationRan, "The State Validator did not run");
        }
    }
}
