using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Bot.Connector;

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

            TestAdapter connector = new TestAdapter();
            Bot bot = new Bot(connector)
                .Use(new EchoMiddleWare());
            
            await connector
                .Send(messageText).AssertReply(messageText)
                .StartTest();
        }       
    }

    public class EchoMiddleWare : IReceiveActivity
    {
        private readonly bool handled; 

        public EchoMiddleWare(bool handled = true)
        {
            this.handled = handled;
        }

        public Task<ReceiveResponse> ReceiveActivity(BotContext context, CancellationToken token)
        {
            var response = context.Request.CreateReply();
            response.Text = context.Request.Text;
            context.Responses.Add(response);
            return Task.FromResult(new ReceiveResponse(this.handled));
        }
    }
}
