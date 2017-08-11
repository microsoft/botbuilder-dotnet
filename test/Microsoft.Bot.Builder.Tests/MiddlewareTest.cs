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
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<IMiddleware, EchoMiddleWare>();
            serviceCollection.UseBotServices().UseTestConnector();

            var testRunner = new TestRunner(serviceCollection);
            await testRunner.Test("test", async (IList<IActivity> responses) =>
            {
                Assert.AreEqual(1, responses.Count);
                Assert.AreEqual("test", (responses.First() as IMessageActivity).Text);
            });
        }

        [TestMethod]
        public async Task Multi_EchoMiddleware_Should_Echo_Multiple()
        {
            int count = 3;
            var serviceCollection = new ServiceCollection();
            for (int i = 0; i < count; i++)
            {
                serviceCollection.AddSingleton<IMiddleware>(new EchoMiddleWare(i == count -1));
            }
            serviceCollection.UseBotServices().UseTestConnector();

            var testRunner = new TestRunner(serviceCollection);
            await testRunner.Test("test", async (IList<IActivity> responses) =>
            {
                Assert.AreEqual(count, responses.Count);
                Assert.AreEqual(count, responses.Cast<IMessageActivity>().Where(r => r.Text == "test").Count());
            });
        }
    }

    public class EchoMiddleWare : IPostToBot
    {
        private readonly bool handled; 

        public EchoMiddleWare(bool handled = true)
        {
            this.handled = handled;
        }

        public Task<bool> ReceiveActivity(BotContext context, CancellationToken token)
        {
            var response = (context.Request as Activity).CreateReply();
            response.Text = (context.Request as Activity).Text;
            context.Responses.Add(response);
            return Task.FromResult(handled);
        }
    }
}
