// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Middleware;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Microsoft.Bot.Builder.Middleware.MiddlewareSet;

namespace Microsoft.Bot.Builder.Tests
{
    [TestClass]
    [TestCategory("Bot")]
    [TestCategory("Functional Spec")]
    public class Bot_FunctionalTests
    {
        [TestMethod]
        public async Task SingleParameterConstructor()
        {
            ActivityAdapterBase adapter = new TestAdapter();
            Bot bot = new Bot(adapter);

            // If this compiles, the test has passed. :) 
        }

        [TestMethod]
        public async Task AdapterProperty()
        {
            TestAdapter adapter = new TestAdapter();
            Bot bot = new Bot(adapter);


            ActivityAdapterBase retrievedAdapter = bot.Adapter;

            // Verify the Bot a property to allow retrieving the Adapter. 
            Assert.AreSame(adapter, retrievedAdapter);
        }

        [TestMethod]
        public async Task RunSendPiplineWith0Response()
        {
            TestAdapter adapter = new TestAdapter();
            Bot bot = new Bot(adapter);
            WasThisMiddlwareCalled testMiddleware = new WasThisMiddlwareCalled();
            bot.Use(testMiddleware)
                .OnReceive(async (context) => { });

            await adapter
                .Send("foo")
                .StartTest();

            // Test that even though the bot didn't reply with anything, that all 3 pipelines
            // were called. This allows (for example) bot state managment to run even if the 
            // bot doesn't return anything. 
            Assert.IsTrue(testMiddleware.WasContextCreatedCalled, "Context Created was not called");
            Assert.IsTrue(testMiddleware.WasRecevieActivityCalled, "Receive was not called");
            Assert.IsTrue(testMiddleware.WasSendActivityCalled, "Send was not called");
        }

        [TestMethod]
        public async Task RunSendPiplineWith1Response()
        {
            TestAdapter adapter = new TestAdapter();
            Bot bot = new Bot(adapter);
            WasThisMiddlwareCalled testMiddleware = new WasThisMiddlwareCalled();
            bot.Use(testMiddleware)
                .OnReceive(async (context) => { context.Reply("one"); });

            await adapter
                .Send("foo").AssertReply("one")
                .StartTest();

            Assert.IsTrue(testMiddleware.WasContextCreatedCalled, "Context Created was not called");
            Assert.IsTrue(testMiddleware.WasRecevieActivityCalled, "Receive was not called");
            Assert.IsTrue(testMiddleware.WasSendActivityCalled, "Send was not called");
        }        

        public class WasThisMiddlwareCalled : IContextCreated, ISendActivity, IReceiveActivity
        {
            public bool WasContextCreatedCalled { get; set; } = false;
            public bool WasRecevieActivityCalled { get; set; } = false;
            public bool WasSendActivityCalled { get; set; } = false;


            public WasThisMiddlwareCalled() { }

            public async Task ContextCreated(IBotContext context, NextDelegate next)
            {
                WasContextCreatedCalled = true;
                await next();
            }

            public async Task SendActivity(IBotContext context, IList<IActivity> activities, NextDelegate next)
            {
                WasSendActivityCalled = true;
                await next();
            }

            public async Task ReceiveActivity(IBotContext context, NextDelegate next)
            {
                WasRecevieActivityCalled = true;
                await next(); ;
            }
        }
    }
}