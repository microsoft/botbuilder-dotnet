using Microsoft.Bot.Builder.Prague;
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

        public static T CreateEmptyContext<T>() where T:IBotContext
        {
            IConnector c = new TestConnector();
            Bot b = new Bot(c);
            Activity a = new Activity();
            if (typeof(T).IsAssignableFrom(typeof(IDialogContext)))
            {
                IDialogContext dc = new DialogContext(b, a);
                return (T)dc;
            }
            if (typeof(T).IsAssignableFrom(typeof(IBotContext)))
            {
                IBotContext bc = new BotContext(b, a);
                return (T)bc;
            }
            else
                throw new ArgumentException($"Unknown Type {typeof(T).Name}");            
        }
    }
}
