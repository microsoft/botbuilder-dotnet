using Microsoft.Bot.Builder;
using Microsoft.Bot.Samples.Middleware;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Microsoft.Bot.Samples.ConsoleConnector
{
    class Program
    {
        static void Main(string[] args)
        {
            MainAsync(args).GetAwaiter().GetResult();
        }

        static async Task MainAsync(string[] args)
        {
            Builder.ConsoleConnector cc = new Builder.ConsoleConnector();
            
            Builder.Bot bot = new Builder.Bot(cc)
                .Use(CreateRegEx())
                .Use(new EchoMiddleWare())
                .Use(new ReverseMiddleWare())
                .Use(new ConsoleLogger());

            await cc.Listen();
        }

        public static RegExpRecognizerMiddleare CreateRegEx()
        {
            RegExpRecognizerMiddleare regExpMiddleware = new RegExpRecognizerMiddleare();
            regExpMiddleware.AddIntent(
                "echoIntent", new Regex("echo", RegexOptions.IgnoreCase));

            regExpMiddleware.AddIntent(
                "reverseIntent", new Regex("reverse", RegexOptions.IgnoreCase));

            return regExpMiddleware;
        }
    }
}
