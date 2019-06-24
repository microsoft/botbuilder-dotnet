namespace Microsoft.Bot.StreamingExtensions.Payloads
{
    internal interface IPayloadTypeManager
    {
        PayloadAssembler CreatePayloadAssembler(Header header);
    }
}
