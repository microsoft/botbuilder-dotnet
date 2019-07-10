using System;
using System.Collections.Generic;

namespace Microsoft.Bot.Builder.Adapters.WeChat
{
    public class WeChatLogger
    {
        public static readonly WeChatLogger Instance = new WeChatLogger();
        private readonly IBotTelemetryClient client;

        public WeChatLogger(IBotTelemetryClient client = null)
        {
            this.client = client ?? NullBotTelemetryClient.Instance;
        }

        public void TrackException(string message, Exception exception, IDictionary<string, string> properties = null, IDictionary<string, double> metrics = null)
        {
            this.TrackTrace(message, Severity.Error);
            this.client.TrackException(exception, properties, metrics);
        }

        public void TrackTrace(string message, Severity level = Severity.Information, IDictionary<string, string> properties = null)
        {
            client.TrackTrace(message, level, properties);
        }
    }
}
