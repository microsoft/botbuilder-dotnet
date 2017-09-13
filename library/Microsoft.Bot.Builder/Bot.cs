using Microsoft.Bot.Connector;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder
{
    public class Bot : MiddlewareSet
    {
        private IConnector _connector;
        private IBotLogger _logger = new NullLogger();

        public Bot(IConnector connector) : base()
        {
            BotAssert.ConnectorNotNull(connector);
            _connector = connector;
            _connector.Bot = this;

            PostToConnectorMiddleware poster = new PostToConnectorMiddleware(this);
            this.Use(poster);
        }

        public Bot Use(IBotLogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException("logger");
            return this;
        }

        public Bot Use (IMiddleware middleware)
        {
            base.Use(middleware);
            return this;
        }

        public IBotLogger Logger => _logger;

        public IConnector Connector
        {
            get
            {
                return _connector;
            }
            set
            {
                /** Changes the bots connector. The previous connector will first be disconnected */
                BotAssert.ConnectorNotNull(value);

                // Disconnect from existing connector
                if (_connector != null)
                {
                    // ToDo: How to cancel any existing async / await here and disconnect? 
                }

                _connector = value;
                _connector.Bot = this;
            }
        }

        public virtual async Task RunPipeline(IActivity activity, CancellationToken token)
        {
            BotAssert.ActivityNotNull(activity);
            BotAssert.CancellationTokenNotNull(token);

            Logger.Information($"Bot: Pipeline Running for Activity {activity.Id}");

            var context = await this.CreateBotContext(activity, token).ConfigureAwait(false);
            await base.RunPipeline(context, token).ConfigureAwait(false);
            Logger.Information($"Bot: Pipeline Complete for Activity {activity.Id}");
        }

        public virtual Task<BotContext> CreateBotContext(IActivity activity, CancellationToken token)
        {
            BotAssert.ActivityNotNull(activity);
            BotAssert.CancellationTokenNotNull(token);

            Logger.Information($"Bot: Creating BotContext for {activity.Id}");

            return Task.FromResult(new BotContext(this, activity));
        }

        public virtual async Task<BotContext> CreateBotContext(ConversationReference reference, CancellationToken token)
        {
            if (reference == null)
                throw new ArgumentNullException("reference");

            BotAssert.CancellationTokenNotNull(token);

            Logger.Information($"Bot: Creating BotContext for {reference.ActivityId}");

            return await this.CreateBotContext(reference.GetPostToBotMessage(), token).ConfigureAwait(false);
        }
    }  
}
