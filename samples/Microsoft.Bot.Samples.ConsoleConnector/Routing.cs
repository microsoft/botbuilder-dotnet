using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Prague;
using static Microsoft.Bot.Builder.Prague.RoutingRules;
using static Microsoft.Bot.Builder.Prague.Routers;

namespace Microsoft.Bot.Samples
{
    public class Routing
    {
        public static IRouter BuildHelpRouting()
        {
            var first = First(
                IfTrue(
                    (context) => context.IfIntent("help"),
                    Simple( (context) => context.Reply("No Help for you!"))
                ));

            return first;
        }

        public static IRouter BuildLoggingRouting()
        {
            var first = First(
                IfTrue(
                    (context) => context.IfIntent("logging"),
                    Simple((context) => EnableOrDisableLogging(context)))
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
