namespace Microsoft.Bot.Builder.Adapters.WeChat
{
    public interface IAttachmentHash
    {
        string Hash(byte[] bytes);

        string Hash(string content);
    }
}
