//using Microsoft.Bot.Builder;
//using Microsoft.Bot.Builder.Conversation;
//using static Microsoft.Bot.Builder.Conversation.RoutingRules;
//using static Microsoft.Bot.Builder.Conversation.Routers;
//using System.Threading.Tasks;

//namespace Microsoft.Bot.Samples
//{
//    public class Routing
//    {
//        public static Router BuildHelpRouting()
//        {
//            var first = TryInOrder(
//                IfTrue(async (context, routePaths) => Task.FromResult(context.IfIntent("help") ? new MatcherResult() { Score = 1.0 } : (MatcherResult)null)))
//                .The
//                    .Then
//                    Simple( (context, result) => context.Reply("No Help for you!"))
//                ));

//            return first;
//        }

//        public static Router BuildLoggingRouting()
//        {
//            var first = TryInOrder(
//                IfTrue(
//                    (context) => Task.FromResult(context.IfIntent("logging") ? new MatcherResult() { Score = 1.0 } : null)),
//                    Simple((context, result) => EnableOrDisableLogging(context)))
//                );

//            return first;
//        }

//        private static void EnableOrDisableLogging(IBotContext context)
//        {
//            // We know the Logging intent has fired. 
//            if (context.Request.Text.Contains("start"))
//                ((ConsoleLogger)context.Logger).LoggingEnabled = true;
//            else if (context.Request.Text.Contains("stop"))
//                ((ConsoleLogger)context.Logger).LoggingEnabled = false;
//        }
//    }
//}
