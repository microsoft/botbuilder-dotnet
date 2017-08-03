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
        private readonly PostToConnectorMiddleware postToConnectorMiddleware;
        
        public Bot(PostToConnectorMiddleware postToConnectorMiddleware)
        {
            SetField.NotNull(out this.postToConnectorMiddleware, nameof(postToConnectorMiddleware), postToConnectorMiddleware);
        }

        public Func<BotContext, CancellationToken, Task<bool>> OnReceive = null;

        public MiddlewareSet MiddlewareSet => postToConnectorMiddleware.MiddlewareSet;

        public async Task<bool> Receive(BotContext context, CancellationToken token = default(CancellationToken))
        {
            var done = false; 
            try
            {
                await this.postToConnectorMiddleware.ContextCreated(context, token);
                done = await this.postToConnectorMiddleware.ReceiveActivity(context, token);
                if(!done && OnReceive != null)
                {
                    done = await this.OnReceive(context, token);
                }
            }
            finally
            {
                await this.postToConnectorMiddleware.ContextDone(context, token);
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

            // Activity resolver and IActivity
            services.AddScoped<ActivityResolver>();
            services.AddScoped<IActivity>(provider => provider.GetRequiredService<ActivityResolver>().Resolve());

            // Setup botContextFactory
            services.AddScoped<IBotContextFactory, BotContextFactory>();
            
            // create default middlewareset
            services.AddScoped<MiddlewareSet>(provider => new MiddlewareSet(provider.GetServices<IMiddleware>().ToList()));

            // wrap all other middlewares registered with the container 
            // with PostToConnectorMiddleware
            services.AddScoped<PostToConnectorMiddleware>();

            // register PostToConnectorMiddleware as IPostToUser
            services.AddScoped<IPostToUser>(provider => provider.GetService<PostToConnectorMiddleware>());
            
            services.AddScoped<Bot>();
            return services;
        }
    }
}
