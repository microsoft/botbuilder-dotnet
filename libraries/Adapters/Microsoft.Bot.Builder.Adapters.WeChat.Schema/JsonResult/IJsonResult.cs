namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema.JsonResult
{
    public interface IJsonResult
    {
        string ErrorMessage { get; set; }

        int ErrorCodeValue { get; }

        object P2PData { get; set; }
    }
}
