namespace Microsoft.Bot.StreamingExtensions.Payloads
{
    public interface IPayloadTypeManager
    {
        PayloadAssembler CreatePayloadAssembler(Header header);
    }
}
