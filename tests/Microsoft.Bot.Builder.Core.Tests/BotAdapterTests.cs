using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Core.Tests
{
    [TestClass]
    [TestCategory("BotAdapter")]
    public class BotAdapterTests
    {
        [TestMethod]        
        public async Task AdapterSingleUse()
        {
            SimpleAdapter a = new SimpleAdapter();
            a.Use(new CallCountingMiddleware()); 

            // Compiled. Test passed. 
        }

        [TestMethod]
        public async Task AdapterUseChaining()
        {
            SimpleAdapter a = new SimpleAdapter();
            a.Use(new CallCountingMiddleware()).Use(new CallCountingMiddleware());
            // Compiled. Test passed. 
        }      
    }

    public class CallCountingMiddleware : IMiddleware
    {
        public int Calls { get; set; }
        public async Task OnProcessRequest(IBotContext context, MiddlewareSet.NextDelegate next)
        {
            Calls++;
            await next();
        }
    }
}
