using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder
{
    public class Bot : MiddlewareSet
    {
        private ActivityAdapterBase _adapter;
        private IBotLogger _logger = new NullLogger();

        public delegate Task<ReceiveResponse> ReceiveDelegate_NoDefault(BotContext context, CancellationToken token);
        public delegate Task ReceiveDelegate_DefaultHandled(BotContext context, CancellationToken token);

        private ReceiveDelegate_NoDefault[] _onReceive = null;

        public Bot OnReceive(params ReceiveDelegate_NoDefault[] receiveHandler)
        {
            if (receiveHandler == null)
                throw new ArgumentNullException(nameof(receiveHandler)); 

            if (receiveHandler.Count() == 0)
                throw new ArgumentOutOfRangeException("No Receive Handlers specified");

            _onReceive = receiveHandler;
            return this;
        }

        public Bot OnReceive(params ReceiveDelegate_DefaultHandled[] receiveHandler)
        {
            if (receiveHandler == null)
                throw new ArgumentNullException(nameof(receiveHandler)); 

            if (receiveHandler.Count() == 0)
                throw new ArgumentOutOfRangeException("No Receive Handlers specified");
            
            IList<ReceiveDelegate_NoDefault> responses = new List<ReceiveDelegate_NoDefault>();            

            // If the user doesn't want to worry about returning Handled / Not Handled, 
            // these will wrap their delegates and always return "Handled". 
            foreach(ReceiveDelegate_DefaultHandled nullReturn in receiveHandler)
            {
                ReceiveDelegate_NoDefault d = async (context, token) =>
                {
                    await nullReturn(context, token);
                    return new ReceiveResponse(true);
                };

                responses.Add(d);
            }

            return OnReceive(responses.ToArray());
        }


        public Bot(ActivityAdapterBase adapter) : base()
        {
            BotAssert.AdapterNotNull(adapter);
            _adapter = adapter;

            // Hook up the Adapter so that incoming data is routed 
            // through the Middleware Pipeline
            _adapter.OnReceive = this.RunPipeline;

            PostToAdapterMiddleware poster = new PostToAdapterMiddleware(this);
            this.Use(poster);
        }

        public Bot Use(IBotLogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger)); 
            return this;
        }

        public Bot Use (IMiddleware middleware)
        {
            base.Use(middleware);
            return this;
        }

        public IBotLogger Logger => _logger;

        public ActivityAdapterBase Adapter
        {
            get
            {
                return _adapter;
            }
            set
            {
                /** Changes the bots connector. The previous connector will first be disconnected */
                BotAssert.AdapterNotNull(value);

                // Disconnect from existing adapter
                if (_adapter != null)
                {
                    // ToDo: How to cancel any existing async / await here and disconnect? 
                }

                _adapter = value;
                _adapter.OnReceive = this.RunPipeline;                
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
                throw new ArgumentNullException(nameof(reference)); 

            BotAssert.CancellationTokenNotNull(token);

            Logger.Information($"Bot: Creating BotContext for {reference.ActivityId}");

            return await this.CreateBotContext(reference.GetPostToBotMessage(), token).ConfigureAwait(false);
        }
    }  
}
