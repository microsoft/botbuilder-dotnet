// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Core.Extensions.Tests
{
    [TestClass]
    [TestCategory("Batch Output")]
    public class BatchOutputTests
    {
        [TestMethod]        
        public async Task Add0Items()
        {
            BotContext c = new BotContext(new TestAdapter(), new Activity());
            BatchOutput bo = new BatchOutput();
            
            c.OnSendActivity( async (context, activities, next) =>
            {
                Assert.IsTrue(activities.Count == 0, "Incorrect Item Count");
            });

            await bo.Flush(c);
        }

        [TestMethod]        
        public async Task Add1Items()
        {
            bool checksPassed = false;

            BotContext c = new BotContext(new TestAdapter(), new Activity());
            BatchOutput bo = new BatchOutput();
            bo.Typing(); 

            c.OnSendActivity(async (context, activities, next) =>
            {
                Assert.IsTrue(activities.Count == 1, "Incorrect Activity Count");
                Assert.IsTrue(activities[0].Type == ActivityTypes.Typing, "Incorrect Activity Type");
                checksPassed = true;
            });

            await bo.Flush(c);
            Assert.IsTrue(checksPassed, "Activities were not validated"); 
        }

        [TestMethod]
        public async Task Send2ItemsInOrder()
        {
            bool checksPassed = false;

            BotContext c = new BotContext(new TestAdapter(), new Activity());
            BatchOutput bo = new BatchOutput();
            bo.Typing();
            bo.EndOfConversation();
           
            c.OnSendActivity(async (context, activities, next) =>
            {
                Assert.IsTrue(activities.Count == 2, "Incorrect Activity Count");
                Assert.IsTrue(activities[0].Type == ActivityTypes.Typing, "Incorrect Activity Type in Slot 0");
                Assert.IsTrue(activities[1].Type == ActivityTypes.EndOfConversation, "Incorrect Activity Type in Slot 0");
                checksPassed = true;
            });

            await bo.Flush(c);
            Assert.IsTrue(checksPassed, "Activities were not validated");
        }

        [TestMethod]
        public async Task BatchOutputMiddlewareTest()
        {
            TestAdapter adapter = new TestAdapter()
                .Use(new BatchOutputMiddleware());

            async Task Echo(IBotContext ctx)
            {
                ctx.Batch().Reply("ECHO:" + ctx.Request.Text);                                 
            }

            await new TestFlow(adapter, Echo)
                .Send("test")                
                .AssertReply("ECHO:test")                
                .StartTest();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ThrowOnForgottenMiddleware()
        {
            TestAdapter adapter = new TestAdapter();
                
            // Note: Did NOT add in the Middleare. This MUST
            // cause the Batch() extension method below to fail
            // as the required state is missing on the Context. 

            async Task DoSomething(IBotContext ctx)
            {
                // Should throw in the Extension Method
                ctx.Batch().Reply("foo");
                Assert.Fail("Should not get here"); 
            }

            await new TestFlow(adapter, DoSomething)
                .Send("foo")
                .StartTest();
        }
    }
}