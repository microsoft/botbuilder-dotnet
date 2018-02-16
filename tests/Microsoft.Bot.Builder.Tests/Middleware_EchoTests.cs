// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Middleware;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Tests
{
    [TestClass]
    [TestCategory("Middleware")]
    public class Middleware_EchoTests
    {
        [TestMethod]        
        public async Task Middleware_Echo()
        {
            string messageText = Guid.NewGuid().ToString();

            
            TestBot bot = new TestBot()
                .Use(new EchoMiddleWare());
            
            await new TestFlow(bot)
                .Send(messageText).AssertReply(messageText)
                .StartTest();
        }       
    }

    public class EchoMiddleWare : Middleware.IReceiveActivity
    {
        private readonly bool handled; 

        public EchoMiddleWare(bool handled = true)
        {
            this.handled = handled;
        }        

        public async Task ReceiveActivity(IBotContext context, MiddlewareSet.NextDelegate next)
        {            
            var response = ((Activity)context.Request).CreateReply();
            response.Text = context.Request.AsMessageActivity().Text;
            context.Responses.Add(response);
            await next();            
        }
    }
}
