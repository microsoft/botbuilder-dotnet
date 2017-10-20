using System;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Conversation
{
    public static class Routers
    {
        /// <summary>
        /// Router that throws an InvalidOperationExcpetion when it's used. 
        /// This router is primarly used for Unit Testing to insure routing
        /// order and proper error handling. 
        /// </summary>
        public static Router Error()
        {
            Router errorRouter = new Router(async (context, routePath) =>
            {
                throw new InvalidOperationException("Error by design");
            });

            return errorRouter;
        }

        public static Handler Simple(Action action)
        {
            return new Handler(action);
        }

        public static Handler Simple(Func<Task> func)
        {
            return new Handler(func);
        }

        public static Router Simple(Action<IBotContext> action)
        {
            Router r = new Router(
                async (context, routePath) =>
                {
                    return new Route(async () => action(context));                    
                });
            return r;
        }

        public static Router Scored(Action action, double score)
        {            
            Router r = new Router(
               async (context, routePath) =>
               {
                   return new Route(async () => action(), score, routePath);
               });
            return r;
        }

        public static Router Scored(Func<Task> func, double score)
        {
            Router r = new Router(
               async (context, routePath) =>
               {
                   return new Route(() => func(), score, routePath);
               });
            return r;
        }
    }      
}