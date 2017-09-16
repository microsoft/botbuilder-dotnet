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

        public delegate Task<ReceiveResponse> ReceiveDelegate(BotContext context, CancellationToken token);
        private ReceiveDelegate[] _onReceive = null;

        public Bot OnReceive(params ReceiveDelegate[] receiveHandler)
        {
            _onReceive = receiveHandler;
            return this;
        }


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

        public override async Task<ReceiveResponse> ReceiveActivity(BotContext context, CancellationToken token)
        {
            BotAssert.ContextNotNull(context);
            BotAssert.CancellationTokenNotNull(token);
            
            var result = await base.ReceiveActivity(context, token);
            if (result?.Handled == false)
            {
                if (_onReceive != null)
                {
                    foreach (var r in _onReceive)
                    {
                        result = await r(context, token);
                        if (result.Handled == true)
                            break;
                    }
                }
            }

            return result;
        }

        public virtual async Task RunPipeline(Activity activity, CancellationToken token)
        {
            BotAssert.ActivityNotNull(activity);
            BotAssert.CancellationTokenNotNull(token);

            Logger.Information($"Bot: Pipeline Running for Activity {activity.Id}");

            var context = await this.CreateBotContext(activity, token).ConfigureAwait(false);
            await base.RunPipeline(context, token).ConfigureAwait(false);
            Logger.Information($"Bot: Pipeline Complete for Activity {activity.Id}");
        }

        public virtual Task<BotContext> CreateBotContext(Activity activity, CancellationToken token)
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
