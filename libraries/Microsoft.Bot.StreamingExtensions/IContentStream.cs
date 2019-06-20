using System;
using System.IO;

namespace Microsoft.Bot.StreamingExtensions
{
    public interface IContentStream
    {
        Stream GetStream();

        Guid Id { get; }

        string Type { get; set; }

        int? Length { get; set; }
    }
}
