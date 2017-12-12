using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Templates;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder
{
    public class Bot : MiddlewareSet
    {
        private ActivityAdapterBase _adapter;
        private IBotLogger _logger = new NullLogger();

        public delegate Task<ReceiveResponse> ReceiveDelegate_NoDefault(BotContext context);
        public delegate Task ReceiveDelegate_DefaultHandled(BotContext context);

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
            foreach (ReceiveDelegate_DefaultHandled nullReturn in receiveHandler)
            {
                ReceiveDelegate_NoDefault d = async (context) =>
                {
                    await nullReturn(context);
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

            // Add templateManager
            this.Use(new TemplateManager());
        }

        public Bot Use(IBotLogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            return this;
        }

        public Bot Use(IMiddleware middleware)
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

        public override async Task<ReceiveResponse> ReceiveActivity(BotContext context)
        {
            BotAssert.ContextNotNull(context);

            var result = await base.ReceiveActivity(context);
            if (result?.Handled == false)
            {
                if (_onReceive != null)
                {
                    foreach (var r in _onReceive)
                    {
                        result = await r(context);
                        if (result.Handled == true)
                            break;
                    }
                }
            }

            return result;
        }

        public virtual async Task RunPipeline(Activity activity)
        {
            BotAssert.ActivityNotNull(activity);

            var context = new BotContext(this, activity);

            await base.RunPipeline(context).ConfigureAwait(false);
        }

        /// <summary>
        /// Create proactive context around conversation reference
        /// All middleware pipelines will be processed
        /// </summary>
        /// <param name="reference">reference to create context around</param>
        /// <param name="proactiveCallback">callback where you can continue the conversation</param>
        /// <returns>task when completed</returns>
        public virtual async Task CreateContext(ConversationReference reference, Func<BotContext, Task> proactiveCallback)
        {
            var context = new BotContext(this, reference);

            await base.RunPipeline(context, proactiveCallback);
        }
    }
}
