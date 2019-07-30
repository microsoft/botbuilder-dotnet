using System;
using System.IO;

//[assembly: System.Runtime.CompilerServices.InternalsVisibleTo(
//    "Microsoft.Bot.StreamingExtensions.Tests.Microsoft.Bot.StreamingExtensions.UnitTests.Mocks, PublicKey=00240000048" +
//    "00000940000000602000000240000525341310004000001000100b5fc90e7027f67871e773a8fde8938c81dd402ba65b9201d60593e96c49" +
//    "2651e889cc13f1415ebb53fac1131ae0bd333c5ee6021672d9718ea31a8aebd0da0072f25d87dba6fc90ffd598ed4da35e44c398c454307e" +
//    "8e33b8426143daec9f596836f97c8f74750e5975c64e2189f45def46b2a2b1247adc3652bf5c308055da9")]
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo(
    "Microsoft.Bot.StreamingExtensions.Tests, PublicKey=0024000004800000940000000602000000240000525341310004000001000100b5fc90e7027f67871e773a8fde8938c81dd402ba65b9201d60593e96c492651e889cc13f1415ebb53fac1131ae0bd333c5ee6021672d9718ea31a8aebd0da0072f25d87dba6fc90ffd598ed4da35e44c398c454307e8e33b8426143daec9f596836f97c8f74750e5975c64e2189f45def46b2a2b1247adc3652bf5c308055da9")]
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo(
    "Microsoft.Bot.StreamingExtensions.Tests")]

namespace Microsoft.Bot.StreamingExtensions.Payloads
{
    internal interface IAssembler
    {
        bool End { get; }

        Guid Id { get; }

        void Close();

        Stream CreateStreamFromPayload();

        Stream GetPayloadAsStream();

        void OnReceive(Header header, Stream stream, int contentLength);
    }
}
