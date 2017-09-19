using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Tests
{
    [TestClass]
    public class MiddlewareTest
    {
        [TestMethod]
        public async Task EchoMiddleware_Should_Echo()
        {
            string messageText = Guid.NewGuid().ToString();

            TestConnector connector = new TestConnector();
            connector.ValidationsToRunOnPost(
                (responses) => Assert.AreEqual(responses.Count, 1),
                (responses) => Assert.AreEqual(messageText, (responses.First() as IMessageActivity).Text)
            );

            Bot bot = new Bot(connector)
                .Use(new EchoMiddleWare());

            var runner = new TestRunner();
            await runner.Test(connector, messageText);
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
