using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Tests
{
    public class TestUtilities
    {
        public static BotContext CreateEmptyContext()
        {
            IConnector c = new TestConnector();
            Bot b = new Bot(c);
            Activity a = new Activity();
            BotContext bc = new BotContext(b, a);

            return bc;
        }
    }
}
