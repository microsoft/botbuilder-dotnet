using Microsoft.Bot.Builder.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Middleware;
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

            TestAdapter adapter = new TestAdapter();
            Bot bot = new Bot(adapter)
                .Use(new EchoMiddleWare());
            
            await adapter
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
