using Microsoft.Bot.Builder.Prague;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Tests
{
    [TestClass]
    public class RouterMiddlewareTests
    {
        [TestMethod]
        public async Task SimpleRoute()
        {
            var engine = new ActivityRoutingMiddleware(
                new SimpleRouter((context) => context.Reply("routed")));

            TestConnector connector = new TestConnector();
            Bot bot = new Bot(connector)
                .Use(engine); 

            await connector.Test("test", (a) => Assert.IsTrue(a[0].Text == "routed"));
        }

        [TestMethod]
        public async Task EvaluteOnlyFirstRule()
        {
            var engine = new ActivityRoutingMiddleware(
                new FirstRouter()
                    .Add(new SimpleRouter((context) => context.Reply("routed")))
                    .Add(new ErrorRouter())
                );

            TestConnector connector = new TestConnector();
            Bot bot = new Bot(connector)
                .Use(engine);

            await connector.Test("test", (a) => Assert.IsTrue(a[0].Text == "routed"));
        }

        [TestMethod]
        public async Task SkipRule()
        {
            var engine = new ActivityRoutingMiddleware(
                new FirstRouter()
                    .Add(new IfMatch((context)=>false, new ErrorRouter() ))
                    .Add(new SimpleRouter((context) => context.Reply("routed")))
                    .Add(new ErrorRouter())
                );

            TestConnector connector = new TestConnector();
            Bot bot = new Bot(connector)
                .Use(engine);

            await connector.Test("test", (a) => Assert.IsTrue(a[0].Text == "routed"));
        }

        [TestMethod]
        public async Task MatchOnElseCase()
        {
            var engine = new ActivityRoutingMiddleware(
                new FirstRouter()
                    .Add(
                        new IfMatch(
                            (context) => false, 
                            new ErrorRouter(), 
                            new SimpleRouter( (context) => context.Reply("routed"))
                            ))                    
                );

            TestConnector connector = new TestConnector();
            Bot bot = new Bot(connector)
                .Use(engine);

            await connector.Test("test", (a) => Assert.IsTrue(a[0].Text == "routed"));
        }
    }
}
