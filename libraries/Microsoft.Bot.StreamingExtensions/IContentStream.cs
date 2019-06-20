using System;
using System.IO;

namespace Microsoft.Bot.StreamingExtensions
{
    public interface IContentStream
    {
        Guid Id { get; }

        string Type { get; set; }

        int? Length { get; set; }

        Stream GetStream();
    }
}
