namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema.Request.Event
{
    public interface IRequestMessageEventBase : IRequestMessageBase
    {
        string Event { get; }
    }
}
