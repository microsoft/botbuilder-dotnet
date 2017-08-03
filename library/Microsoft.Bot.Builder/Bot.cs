using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Bot.Builder
{
    public class Bot
    {
        private readonly MiddlewareSet middlewareSet;
        
        public Bot(MiddlewareSet middlewareSet)
        {
            SetField.NotNull(out this.middlewareSet, nameof(middlewareSet), middlewareSet);
        }

        public Func<BotContext, CancellationToken, Task<bool>> OnReceive = null;

        public MiddlewareSet MiddlewareSet => middlewareSet;

        public async Task<bool> Receive(BotContext context, CancellationToken token = default(CancellationToken))
        {
            var done = false; 
            try
            {
                await this.middlewareSet.ContextCreated(context, token);
                done = await this.middlewareSet.ReceiveActivity(context, token);
                if(!done && OnReceive != null)
                {
                    done = await this.OnReceive(context, token);
                }
            }
            finally
            {
                await this.middlewareSet.ContextDone(context, token);
            }
            return done;
        }
    }
    
    public static partial class BotExtensions
    {
        public static void Use(this Bot bot, params IMiddleware[] middlewares)
        {
            foreach (var middlerware in middlewares)
            {
                bot.MiddlewareSet.Middlewares.Add(middlerware);
            }
        }

        public static IServiceCollection UseBotServices(this IServiceCollection services)
        {
            // BotLogger
            services.AddSingleton<IBotLogger, NullLogger>();

            // Setup dataContext
            services.AddScoped<IDataContext, NullDataContext>();

            // Setup botContextFactory
            services.AddScoped<IBotContextFactory, BotContextFactory>();
            
            // add post to connector as the default middleware
            services.AddScoped<PostToConnectorMiddleWare>();
            services.AddScoped<IMiddleware>(provider => provider.GetService<PostToConnectorMiddleWare>())
                    .AddScoped<IList<IMiddleware>>(provider => provider.GetServices<IMiddleware>().ToList());

            // register middleware set and set it up as IPostToUser
            services.AddScoped<MiddlewareSet>()
                    .AddScoped<IPostToUser>(provider => provider.GetService<MiddlewareSet>());
            
            services.AddScoped<Bot>();
            return services;
        }
    }

    public class PostToConnectorMiddleWare : IPostToUser, IContextFinalizer
    {
        private readonly IConnector connector;

        public PostToConnectorMiddleWare(IConnector connector)
        {
            SetField.NotNull(out this.connector, nameof(connector), connector);
        }
        
        public async Task ContextDone(BotContext context, CancellationToken token)
        {
            await this.FlushResponses(context, token);
        }

        public async Task PostAsync(BotContext context, IList<IActivity> activities, CancellationToken token)
        {
            
            foreach(var activity in activities)
            {
                context.Responses.Add(activity);
            }
            await this.FlushResponses(context, token);
        }

        private async Task FlushResponses(BotContext context, CancellationToken token)
        {
            await this.connector.Post(context.Responses, token);
            context.Responses.Clear();
        }
    }
}
