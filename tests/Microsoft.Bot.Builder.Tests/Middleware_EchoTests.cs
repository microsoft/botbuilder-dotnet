using Microsoft.Bot.Builder.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

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

            TestAdapter adapter = new TestAdapter();
            Bot bot = new Bot(adapter)
                .Use(new EchoMiddleWare());
            
            await adapter
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

        public Task<ReceiveResponse> ReceiveActivity(BotContext context)
        {
            var response = context.Request.CreateReply();
            response.Text = context.Request.Text;
            context.Responses.Add(response);
            return Task.FromResult(new ReceiveResponse(this.handled));
        }
    }
}
