using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Prague;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;

namespace Microsoft.Bot.Builder.Tests
{
    [TestClass]
    public class Routing_Middleware
    {
        [TestMethod]
        [TestCategory("Middleware")]
        [TestCategory("Routing - Basic")]
        public async Task MiddlwareRouting_SimpleRoute()
        {
            var engine = new ActivityRoutingMiddleware(
                new SimpleRouter((context) => context.Reply("routed")));

            TestAdapter adapter = new TestAdapter();
            Bot bot = new Bot(adapter)
                .Use(engine);

            await adapter.Test("test", "routed")
                .StartTest();
        }

        [TestMethod]
        [TestCategory("Middleware")]
        [TestCategory("Routing - Basic")]
        public async Task MiddlwareRouting_EvaluteOnlyFirstRule()
        {
            var engine = new ActivityRoutingMiddleware(
                new FirstRouter()
                    .Add(new SimpleRouter((context) => context.Reply("routed")))
                    .Add(new ErrorRouter())
                );

            TestAdapter adapter = new TestAdapter();
            Bot bot = new Bot(adapter)
                .Use(engine);

            await adapter
                .Test("test", "routed")
                .StartTest();
        }

        [TestMethod]
        [TestCategory("Middleware")]
        [TestCategory("Routing - Basic")]
        public async Task MiddlwareRouting_SkipRule()
        {
            var engine = new ActivityRoutingMiddleware(
                new FirstRouter()
                    .Add(new IfMatch((context) => false, new ErrorRouter()))
                    .Add(new SimpleRouter((context) => context.Reply("routed")))
                    .Add(new ErrorRouter())
                );

            TestAdapter adapter = new TestAdapter();
            Bot bot = new Bot(adapter)
                .Use(engine);

            await adapter.Test("test", "routed")
                .StartTest();
        }

        [TestMethod]
        [TestCategory("Middleware")]
        [TestCategory("Routing - Basic")]
        public async Task MiddlwareRouting_MatchOnElseCase()
        {
            var engine = new ActivityRoutingMiddleware(
                new FirstRouter()
                    .Add(
                        new IfMatch(
                            (context) => false,
                            new ErrorRouter(),
                            new SimpleRouter((context) => context.Reply("routed"))
                            ))
                );

            TestAdapter adapter = new TestAdapter();
            Bot bot = new Bot(adapter)
                .Use(engine);

            await adapter.Test("test", "routed")
                .StartTest();
        }
    }
}
