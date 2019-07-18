using System;
using Microsoft.ApplicationInsights.DataContracts;

namespace Microsoft.Bot.Builder.Adapters.WeChat
{
    public interface IWeChatLogger
    {
        void TrackTrace(string message, SeverityLevel level);

        void TrackException(string message, Exception exception, SeverityLevel level);
    }
}