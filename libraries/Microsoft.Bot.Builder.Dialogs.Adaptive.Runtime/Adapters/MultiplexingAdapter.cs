using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Runtime.Settings;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Microsoft.Bot.Builder.Integration.AspNet.Core
{
    /// <summary>
    /// A multiplexing bot adapter that can select the appropriate IbotFrameworkHttpAdapter based on the incoming
    /// request's route or the activity's ChannelId.
    /// </summary>
    /// <remarks>
    /// To use this adapter properly, the api/{route} must match the Activity.ChannelId to map the appropriate adapter.
    /// Otherwise, you may override the BuildAdapterMap() method.
    /// </remarks>
    public class MultiplexingAdapter : BotAdapter, IBotFrameworkHttpAdapter
    {
        private readonly Dictionary<string, IBotFrameworkHttpAdapter> _adapters = new Dictionary<string, IBotFrameworkHttpAdapter>();
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiplexingAdapter"/> class.
        /// </summary>
        /// <param name="configuration">An <see cref="IConfiguration"/> instance.</param>
        /// <param name="adapters">The array of adapters used in multiplexing.</param>
        /// /// <param name="logger">The ILogger implementation this adapter should use.</param>
        public MultiplexingAdapter(IConfiguration configuration, IEnumerable<IBotFrameworkHttpAdapter> adapters, ILogger logger = null)
        {
            _configuration = configuration;
            _logger = logger ?? NullLogger.Instance;

            Initialize(adapters);
        }

        /// <inheritdoc/>
        public Task ProcessAsync(HttpRequest httpRequest, HttpResponse httpResponse, IBot bot, CancellationToken cancellationToken = default)
        {
            var routeAsChannelId = httpRequest.Path.ToString().Split('/').LastOrDefault();
            return (GetAppropriateAdapter(routeAsChannelId) as IBotFrameworkHttpAdapter).ProcessAsync(httpRequest, httpResponse, bot, cancellationToken);
        }

        /// <inheritdoc/>
        public override Task DeleteActivityAsync(ITurnContext turnContext, ConversationReference reference, CancellationToken cancellationToken)
        {
            return GetAppropriateAdapter(turnContext.Activity.ChannelId).DeleteActivityAsync(turnContext, reference, cancellationToken);
        }

        /// <inheritdoc/>
        public override Task<ResourceResponse[]> SendActivitiesAsync(ITurnContext turnContext, Activity[] activities, CancellationToken cancellationToken)
        {
            return GetAppropriateAdapter(turnContext.Activity.ChannelId).SendActivitiesAsync(turnContext, activities, cancellationToken);
        }

        /// <inheritdoc/>
        public override Task<ResourceResponse> UpdateActivityAsync(ITurnContext turnContext, Activity activity, CancellationToken cancellationToken)
        {
            return GetAppropriateAdapter(turnContext.Activity.ChannelId).UpdateActivityAsync(turnContext, activity, cancellationToken);
        }

        /// <inheritdoc/>
        public override Task ContinueConversationAsync(string botId, Activity continuationActivity, BotCallbackHandler callback, CancellationToken cancellationToken)
        {
            return GetAppropriateAdapter(continuationActivity.ChannelId).ContinueConversationAsync(botId, continuationActivity, callback, cancellationToken);
        }

        /// <inheritdoc/>
        public override Task ContinueConversationAsync(string botId, ConversationReference reference, BotCallbackHandler callback, CancellationToken cancellationToken)
        {
            return GetAppropriateAdapter(reference.ChannelId).ContinueConversationAsync(botId, reference, callback, cancellationToken);
        }

        /// <inheritdoc/>
        public override Task ContinueConversationAsync(ClaimsIdentity claimsIdentity, Activity continuationActivity, BotCallbackHandler callback, CancellationToken cancellationToken)
        {
            return GetAppropriateAdapter(continuationActivity.ChannelId).ContinueConversationAsync(claimsIdentity, continuationActivity, callback, cancellationToken);
        }

        /// <inheritdoc/>
        public override Task ContinueConversationAsync(ClaimsIdentity claimsIdentity, Activity continuationActivity, string audience, BotCallbackHandler callback, CancellationToken cancellationToken)
        {
            return GetAppropriateAdapter(continuationActivity.ChannelId).ContinueConversationAsync(claimsIdentity, continuationActivity, audience, callback, cancellationToken);
        }

        /// <inheritdoc/>
        public override Task ContinueConversationAsync(ClaimsIdentity claimsIdentity, ConversationReference reference, BotCallbackHandler callback, CancellationToken cancellationToken)
        {
            return GetAppropriateAdapter(reference.ChannelId).ContinueConversationAsync(claimsIdentity, reference, callback, cancellationToken);
        }

        /// <inheritdoc/>
        public override Task ContinueConversationAsync(ClaimsIdentity claimsIdentity, ConversationReference reference, string audience, BotCallbackHandler callback, CancellationToken cancellationToken)
        {
            return GetAppropriateAdapter(reference.ChannelId).ContinueConversationAsync(claimsIdentity, reference, audience, callback, cancellationToken);
        }

        /// <inheritdoc/>
        public override Task CreateConversationAsync(string botAppId, string channelId, string serviceUrl, string audience, ConversationParameters conversationParameters, BotCallbackHandler callback, CancellationToken cancellationToken)
        {
            return GetAppropriateAdapter(channelId).CreateConversationAsync(botAppId, channelId, serviceUrl, audience, conversationParameters, callback, cancellationToken);
        }

        /// <inheritdoc/>
        public override Task<InvokeResponse> ProcessActivityAsync(ClaimsIdentity claimsIdentity, Activity activity, BotCallbackHandler callback, CancellationToken cancellationToken)
        {
            return GetAppropriateAdapter(activity.ChannelId).ProcessActivityAsync(claimsIdentity, activity, callback, cancellationToken);
        }

        /// <summary>
        /// Maps the incoming ChannelId or route (api/{route}) to the correct adapter. By default, this uses the AdapterSettings specified in appsettings.json.
        /// Override this method to customize the mapping behavior.
        /// </summary>
        /// <param name="adapters">The adapters to map the routes to.</param>
        protected virtual void BuildAdapterMap(IEnumerable<IBotFrameworkHttpAdapter> adapters)
        {
            var adapterSettings = _configuration.GetSection(AdapterSettings.AdapterSettingsKey).Get<List<AdapterSettings>>() ?? new List<AdapterSettings>();
            var messagesAdapter = adapterSettings.FirstOrDefault(s => s.Route == "messages");
            if (messagesAdapter == null)
            {
                adapterSettings.Add(AdapterSettings.CoreBotAdapterSettings);
            }

            foreach (var adapter in adapters ?? throw new ArgumentNullException(nameof(adapters)))
            {
                var settings = adapterSettings.FirstOrDefault(s => s.Enabled && s.Type == adapter.GetType().FullName.Split('.').Last());

                if (settings != null)
                {
                    // Map route/ChannelId to adapter.
                    _adapters.Add(settings.Route, adapter);

                    // Add the adapter handling the messages route as the default adapter.
                    if (settings.Route == "messages")
                    {
                        _adapters.Add("default", adapter);
                    }
                }
            }
        }

        private void Initialize(IEnumerable<IBotFrameworkHttpAdapter> adapters)
        {
            BuildAdapterMap(adapters);
        }

        private BotAdapter GetAppropriateAdapter(string channelId)
        {
            if (_adapters.TryGetValue(channelId, out var adapter))
            {
                return adapter as BotAdapter;
            }

            if (_adapters.TryGetValue("default", out var defaultAdapter))
            {
                _logger.LogWarning($"Unable to find router for channelId {channelId}. Using default adapter {nameof(defaultAdapter)}. You may need to ensure your appsettings.json has an adapter with the Route property that matches the ChannelId of the adapter.");
                return defaultAdapter as BotAdapter;
            }

            throw new ArgumentException($"No adapter available for channelId/route '{channelId}' and no default adapter exists");
        }
    }
}
