// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Runtime.Tests.Resources
{
    public class JsonResource : MemoryResource
    {
        private const int BufferSize = 1024;

        private static readonly Encoding Encoding = new UTF8Encoding(
            encoderShouldEmitUTF8Identifier: false,
            throwOnInvalidBytes: true);

        private static readonly JsonSerializer JsonSerializer = JsonSerializer.CreateDefault(
            settings: new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });

        private readonly object data;

        public JsonResource(string id, object data)
            : base(id)
        {
            this.data = data ?? throw new ArgumentNullException(nameof(data));
        }

        public override Task<Stream> OpenStreamAsync()
        {
            Stream stream = new MemoryStream();

            using (var streamWriter = new StreamWriter(stream, Encoding, BufferSize, leaveOpen: true))
            using (var jsonTextWriter = new JsonTextWriter(streamWriter))
            {
                JsonSerializer.Serialize(jsonTextWriter, this.data);
                jsonTextWriter.Flush();
                stream.Position = 0;
            }

            return Task.FromResult(stream);
        }
    }
}
