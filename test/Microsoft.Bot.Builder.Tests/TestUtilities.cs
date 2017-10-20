using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Adapters;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Tests
{
    public class TestUtilities
    {
        public static BotContext CreateEmptyContext()
        {
            ActivityAdapterBase c = new TestAdapter();
            Bot b = new Bot(c);
            Activity a = new Activity();
            BotContext bc = new BotContext(b, a);

            return bc;
        }

        public static T CreateEmptyContext<T>() where T:IBotContext
        {
            var adapter = new TestAdapter();
            Bot b = new Bot(adapter);
            Activity a = new Activity();
            if (typeof(T).IsAssignableFrom(typeof(IBotContext)))
            {
                IBotContext bc = new BotContext(b, a);
                return (T)bc;
            }
            else
                throw new ArgumentException($"Unknown Type {typeof(T).Name}");            
        }

        static Lazy<Dictionary<string, string>> environmentKeys = new Lazy<Dictionary<string, string>>(()=>
        {
            try
            {
                return File.ReadAllLines(@"\\fusebox\private\sdk\UnitTestKeys.cmd")
                    .Where(l => l.StartsWith("@set"))
                    .Select(l => l.Replace("@set ", "").Split('='))
                    .ToDictionary(pairs => pairs[0], pairs => pairs[1]);
            }
            catch (Exception err)
            {
                Debug.WriteLine(err.Message);
                return new Dictionary<string, string>();
            }
        });

        public static string GetKey(string key)
        {
            string value = null;
            environmentKeys.Value.TryGetValue(key, out value);
            return value;
        }

        
    }
}
