using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Prague;
using static Microsoft.Bot.Builder.Prague.RoutingRules;

namespace Microsoft.Bot.Samples
{
    public class Routing
    {
        public static IRouter BuildHelpRouting()
        {
            var first = First(
                new IfMatch(
                    (context) => context.IfIntent("help"),
                    new SimpleRouter((context) => context.Reply("No Help for you!")))
                );

            return first;
        }

        public static IRouter BuildLoggingRouting()
        {
            var first = First(
                new IfMatch(
                    (context) => context.IfIntent("logging"),
                    new SimpleRouter((context) => EnableOrDisableLogging(context)))
                );

            return first;
        }

        private static void EnableOrDisableLogging(IBotContext context)
        {
            // We know the Logging intent has fired. 
            if (context.Request.Text.Contains("start"))
                ((ConsoleLogger)context.Logger).LoggingEnabled = true;
            else if (context.Request.Text.Contains("stop"))
                ((ConsoleLogger)context.Logger).LoggingEnabled = false;
        }
    }
}
