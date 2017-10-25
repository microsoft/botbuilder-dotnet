using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Conversation;
using Microsoft.Bot.Builder.Adapters;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Conversation.Tests
{
    public class DialogTestUtilities
    {

        public static T CreateEmptyContext<T>() where T:IBotContext
        {
            var adapter = new TestAdapter();
            Bot b = new Bot(adapter);
            Activity a = new Activity();
            //if (typeof(T).IsAssignableFrom(typeof(IDialogContext)))
            //{
            //    IDialogContext dc = new DialogContext(b, a);
            //    return (T)dc;
            //}
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
